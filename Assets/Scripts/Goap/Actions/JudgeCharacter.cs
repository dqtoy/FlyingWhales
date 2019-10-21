using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JudgeCharacter : GoapAction {

    public JudgeCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.JUDGE_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //};
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            WeightedFloatDictionary<string> weights = new WeightedFloatDictionary<string>();

            Character targetCharacter = poiTarget as Character;

            float absolve = 0f;
            float whip = 0f;
            float kill = 0f;
            float exile = 0f;

            //base weights
            if (targetCharacter.faction != actor.faction && targetCharacter.faction.GetRelationshipWith(actor.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                //hostile faction
                whip = 5f;
                kill = 100f;
                exile = 10f;
            } else {
                //criminal traits
                CRIME_CATEGORY crime;
                if (targetCharacter.TryGetMostSeriousCrime(out crime)) {
                    switch (crime) {
                        case CRIME_CATEGORY.MISDEMEANOR:
                            absolve = 50f;
                            whip = 100f;
                            break;
                        case CRIME_CATEGORY.SERIOUS:
                            absolve = 5f;
                            whip = 20f;
                            kill = 50f;
                            exile = 50f;
                            break;
                        case CRIME_CATEGORY.HEINOUS:
                            whip = 5f;
                            kill = 100f;
                            exile = 50f;
                            break;
                    }
                } else {
                    throw new System.Exception(actor.name + " is trying to judge " + targetCharacter.name + " but has no crime, and is not part of a hostile faction.");
                }
            }

            //modifiers
            if (targetCharacter.faction == actor.faction) {
                absolve *= 1.5f;
                whip *= 1.5f;
            } else {
                FACTION_RELATIONSHIP_STATUS rel = actor.faction.GetRelationshipWith(targetCharacter.faction).relationshipStatus;
                switch (rel) {
                    case FACTION_RELATIONSHIP_STATUS.COLD_WAR:
                        absolve *= 0.5f;
                        whip *= 0.5f;
                        kill *= 1.5f;
                        exile *= 2f;
                        break;
                    case FACTION_RELATIONSHIP_STATUS.HOSTILE:
                        absolve *= 0.2f;
                        whip *= 0.5f;
                        kill *= 2f;
                        exile *= 1.5f;
                        break;
                }
            }

            List<RELATIONSHIP_TRAIT> rels = actor.GetAllRelationshipTraitTypesWith(targetCharacter);
            if (rels != null) {
                for (int i = 0; i < rels.Count; i++) {
                    switch (rels[i]) {
                        case RELATIONSHIP_TRAIT.LOVER:
                            absolve *= 2f;
                            whip *= 2f;
                            kill *= 0.2f;
                            exile *= 0.5f;
                            break;
                        case RELATIONSHIP_TRAIT.FRIEND:
                        case RELATIONSHIP_TRAIT.RELATIVE:
                            absolve *= 2f;
                            whip *= 2f;
                            kill *= 0.5f;
                            exile *= 0.5f;
                            break;
                        case RELATIONSHIP_TRAIT.ENEMY:
                            absolve *= 0.2f;
                            whip *= 0.5f;
                            kill *= 2f;
                            exile *= 1.5f;
                            break;
                    }
                }
            }

            weights.AddElement("Target Released", absolve);
            weights.AddElement("Target Executed", kill);
            weights.AddElement("Target Exiled", exile);
            weights.AddElement("Target Whip", whip);

            weights.LogDictionaryValues(GameManager.Instance.TodayLogString() + actor.name + " judgement weights towards " + targetCharacter.name);
            string chosen = weights.PickRandomElementGivenWeights();
            SetState(chosen);
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    private void PreTargetExecuted() {
        currentState.SetIntelReaction(EndState1Reactions);
    }
    public void AfterTargetExecuted() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        //**Effect 1**: Remove target's Restrained trait
        //**Effect 2**: Target dies
        (poiTarget as Character).Death("executed", deathFromAction: this, responsibleCharacter: actor);

        RemoveTraitFrom(poiTarget, "Restrained");
    }
    private void PreTargetReleased() {
        currentState.SetIntelReaction(EndState2Reactions);
    }
    public void AfterTargetReleased() {
        //**Effect 2**: If target is from a different faction or unaligned, target is not hostile with characters from the Actor's faction until Target leaves the location. Target is forced to create a Return Home plan
        if (poiTarget.factionOwner == FactionManager.Instance.neutralFaction || poiTarget.factionOwner != actor.faction) {
            ForceTargetReturnHome();
        }
        //**Effect 3**: If target is from the same faction, remove any Criminal type trait from him.
        else {
            RemoveTraitsOfType(poiTarget, TRAIT_TYPE.CRIMINAL);
        }
        //**Effect 1**: Remove target's Restrained trait
        RemoveTraitFrom(poiTarget, "Restrained");
    }
    private void PreTargetExiled() {
        currentState.SetIntelReaction(EndState3Reactions);
    }
    public void AfterTargetExiled() {
        //**Effect 2**: Target becomes unaligned and will have his Home Location set to a random different location
        Character target = poiTarget as Character;
        target.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        //List<Area> choices = new List<Area>(LandmarkManager.Instance.allNonPlayerAreas.Where(x => x.owner == null)); //limited choices to only use un owned areas
        List<Region> choices = GridMap.Instance.allRegions.Where(x => !x.coreTile.isCorrupted).ToList();
        if (choices == null || choices.Count <= 0) {
            choices = GridMap.Instance.allRegions.Where(x => x != PlayerManager.Instance.player.playerArea.region).ToList();
        }
        choices.Remove(target.homeRegion);
        Region newHome = choices[Random.Range(0, choices.Count)];
        target.MigrateHomeTo(newHome);

        //**Effect 3**: Target is not hostile with characters from the Actor's faction until Target leaves the location. Target is forced to create a Return Home plan
        ForceTargetReturnHome();

        //**Effect 4**: Remove any Criminal type trait from him.
        RemoveTraitsOfType(target, TRAIT_TYPE.CRIMINAL);

        //**Effect 1**: Remove target's Restrained trait
        RemoveTraitFrom(poiTarget, "Restrained");
    }
    public void PreTargetWhip() {

    }
    public void AfterTargetWhip() {
        GoapPlanJob whipJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.WHIP);
        actor.jobQueue.AddJobInQueue(whipJob);
    }
    #endregion

    private void ForceTargetReturnHome() {
        Character target = poiTarget as Character;
        target.AdjustIgnoreHostilities(1); //target should ignore hostilities or be ignored by other hostiles, until it returns home.
        target.AddOnLeaveAreaAction(() => target.AdjustIgnoreHostilities(-1));
        target.AddOnLeaveAreaAction(() => target.ClearAllAwarenessOfType(POINT_OF_INTEREST_TYPE.ITEM, POINT_OF_INTEREST_TYPE.TILE_OBJECT));

        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.RETURN_HOME, CHARACTER_STATE.MOVE_OUT);
        target.jobQueue.AddJobInQueue(job);
    }

    #region Intel Reactions
    private List<string> EndState1Reactions(Character recipient, Intel intel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(poiTargetAlterEgo);

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        //Recipient considers Target a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Target Name] deserves that!"
            reactions.Add(string.Format("{0} deserves that!", target.name));
            //-**Recipient Effect * *: no effect
        }
        //Recipient considers Actor a personal Enemy:
         else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] is truly ruthless."
            reactions.Add(string.Format("{0} is truly ruthless.", actor.name));
            //-**Recipient Effect * *: no effect
        }
        //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
        else if (recipient.HasAnyRelationshipOfTypeWith(poiTargetAlterEgo, false, RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.PARAMOUR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "I cannot forgive [Actor Name] for executing [Target Name]!"
            reactions.Add(string.Format("I cannot forgive {0} for executing {1}!", actor.name, target.name));
            //-**Recipient Effect * *:  Recipient will consider Actor an Enemy
            if (!recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
            }
        }
        //Recipient and Target have no relationship but are from the same faction:
        else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
            //- **Recipient Response Text**: "That is sad but I trust that it was a just judgment."
            reactions.Add("That is sad but I trust that it was a just judgment.");
            //-**Recipient Effect * *: no effect
        }
        return reactions;
    }
    private List<string> EndState2Reactions(Character recipient, Intel intel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(poiTargetAlterEgo);

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        //Recipient and Target are the same
        else if (recipient == target) {
            //- **Recipient Response Text**: "I am relieved that I was released."
            reactions.Add("I am relieved that I was released.");
            //-**Recipient Effect**:  no effect
        }
        //Recipient considers Target a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Target Name] shouldn't have been let go so easily!"
            reactions.Add(string.Format("{0} shouldn't have been let go so easily!", target.name));
            //- **Recipient Effect**: If they don't have any relationship yet, Recipient will consider Actor an Enemy
            if (!recipient.HasRelationshipWith(actorAlterEgo, true)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
            }
        }
        //Recipient considers Actor a personal Enemy:
         else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] is simply naive."
            reactions.Add(string.Format("{0} is simply naive.", actor.name));
            //-**Recipient Effect * *: no effect
        }
        //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
        else if (recipient.HasAnyRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.PARAMOUR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "I am grateful that [Actor Name] released [Target Name] unharmed."
            reactions.Add(string.Format("I am grateful that {0} released {1} unharmed.", actor.name, target.name));
            //- **Recipient Effect**:  If they don't have any relationship yet, Recipient will consider Actor a Friend
            if (!recipient.HasRelationshipWith(actorAlterEgo, true)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND);
            }
        }
        //Recipient and Target have no relationship but are from the same faction:
        else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
            //- **Recipient Response Text**: "I trust that it was a just judgment."
            reactions.Add("I trust that it was a just judgment.");
            //-**Recipient Effect * *: no effect
        }
        return reactions;
    }
    private List<string> EndState3Reactions(Character recipient, Intel intel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(poiTargetAlterEgo);

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        //Recipient and Target are the same
        else if (recipient == target) {
            //- **Recipient Response Text**: "I am sad that I was exiled but at least I am still alive."
            reactions.Add("I am sad that I was exiled but at least I am still alive.");
            //-**Recipient Effect**:  no effect
        }
        //Recipient considers Target a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Target Name] shouldn't have been let go so easily!"
            reactions.Add(string.Format("{0} shouldn't have been let go so easily!", target.name));
            //- **Recipient Effect**: no effect
        }
        //Recipient considers Actor a personal Enemy:
         else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] is simply naive."
            reactions.Add(string.Format("{0} is irrational.", actor.name));
            //-**Recipient Effect * *: no effect
        }
        //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
        else if (recipient.HasAnyRelationshipOfTypeWith(poiTargetAlterEgo, false, RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.PARAMOUR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "I am grateful that [Actor Name] exiled [Target Name] unharmed."
            reactions.Add(string.Format("I am grateful that {0} exiled {1} unharmed.", actor.name, target.name));
            //- **Recipient Effect**:  no effect
        }
        //Recipient and Target have no relationship but are from the same faction:
        else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
            //- **Recipient Response Text**: "I trust that it was a just judgment."
            reactions.Add("I trust that it was a just judgment.");
            //-**Recipient Effect * *: no effect
        }
        return reactions;
    }
    #endregion
}

public class JudgeCharacterData : GoapActionData {
    public JudgeCharacterData() : base(INTERACTION_TYPE.JUDGE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
    }
}