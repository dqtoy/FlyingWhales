using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InviteToMakeLove : GoapAction {

    public InviteToMakeLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (!isTargetMissing && targetCharacter.IsInOwnParty()) {
            if (!targetCharacter.isStarving && !targetCharacter.isExhausted
                && targetCharacter.GetNormalTrait("Annoyed") == null && !targetCharacter.HasOtherCharacterInParty()) {
                SetState("Invite Success");
            } else {
                SetState("Invite Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
        if (currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT)
        {
            return Utilities.rng.Next(15, 36);
        }
        return Utilities.rng.Next(30, 56);
    }
    #endregion

    #region Effects
    private void PreInviteSuccess() {
        resumeTargetCharacterState = false;
        Character target = poiTarget as Character;
        bool isParamour = actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR);
        List<TileObject> validBeds = new List<TileObject>();
        //if the characters are paramours
        if (isParamour) {
            //check if they have lovers
            bool actorHasLover = actor.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false);
            bool targetHasLover = target.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false);
            //if one of them doesn't have any lovers
            if (!actorHasLover || !targetHasLover) {
                Character characterWithoutLover;
                if (!actorHasLover) {
                    characterWithoutLover = actor;
                } else {
                    characterWithoutLover = target;
                }
                //pick the bed of the character that doesn't have a lover
                validBeds.AddRange(characterWithoutLover.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
            }
            //if both of them have lovers
            else {
                //check if any of their lovers are currently at the structure that their bed is at
                //if they are not, add that bed to the choices
                Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                if (actorLover.currentStructure != actor.homeStructure) {
                    //lover is not at home structure, add bed to valid choices
                    validBeds.AddRange(actor.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                }
                Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                if (targetLover.currentStructure != target.homeStructure) {
                    //lover is not at home structure, add bed to valid choices
                    validBeds.AddRange(target.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                }
            }
        } else {
            validBeds.AddRange(actor.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
        }

        //if no beds are valid from the above logic.
        if (validBeds.Count == 0) {
            //pick a random bed in a structure that is unowned (No residents)
            List<LocationStructure> unownedStructures = actor.homeArea.GetStructuresAtLocation(true).Where(x => (x is Dwelling && (x as Dwelling).residents.Count == 0)
            || x.structureType == STRUCTURE_TYPE.INN).ToList();

            for (int i = 0; i < unownedStructures.Count; i++) {
                validBeds.AddRange(unownedStructures[i].GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
            }
        }
        IPointOfInterest chosenBed = validBeds[Random.Range(0, validBeds.Count)];

        MakeLove makeLove = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.MAKE_LOVE, actor, chosenBed, false) as MakeLove;
        makeLove.SetTargetCharacter(target);
        makeLove.Initialize();
        GoapNode startNode = new GoapNode(null, makeLove.cost, makeLove);
        GoapPlan makeLovePlan = new GoapPlan(startNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY }, GOAP_CATEGORY.HAPPINESS);
        makeLovePlan.ConstructAllNodes();
        actor.AddPlan(makeLovePlan, true);
        AddTraitTo(target, "Wooed", actor);
        actor.ownParty.AddCharacter(target);
        currentState.SetIntelReaction(State1Reactions);
    }
    private void PreInviteFail() {
        //**After Effect 1**: Actor gains Annoyed trait.
        AddTraitTo(actor, "Annoyed", actor);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        Character target = poiTarget as Character;
        if (target == actor) {
            return false;
        }
        if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            return false;
        }
        if (target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        if (!actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER) && !actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
            return false; //only lovers and paramours can make love
        }
        return target.IsInOwnParty();
    }
    #endregion

    #region Intel Reactions
    private List<string> State1Reactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        RELATIONSHIP_EFFECT recipientWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT recipientWithTarget = recipient.GetRelationshipEffectWith(target);
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);

        //Recipient and Actor are the same or Recipient and Target are the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        //Recipient considers Actor as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "[Actor Name] is cheating on me!?"
            reactions.Add(string.Format("{0} is cheating on me!?", actor.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Target. 
            CharacterManager.Instance.RelationshipDegradation(poiTargetAlterEgo, recipient, this);
            //Add an Undermine Job to Recipient versus Target (choose at random). 
            recipient.ForceCreateUndermineJob(target, "cheated");
            //Add a Breakup Job to Recipient versus Actor.
            recipient.CreateBreakupJob(actor);
            //Add Heartbroken and Betrayed trait to Recipient.
            recipient.AddTrait("Betrayed", actor);
            recipient.AddTrait("Heartbroken", actor);
        }
        //Recipient considers Target as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "[Target Name] is cheating on me!?"
            reactions.Add(string.Format("{0} is cheating on me!?", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor. 
            CharacterManager.Instance.RelationshipDegradation(actorAlterEgo, recipient, this);
            //Add an Undermine Job to Recipient versus Actor.
            recipient.ForceCreateUndermineJob(actor, "cheated");
            //Add a Breakup Job to Recipient versus Target.
            recipient.CreateBreakupJob(target);
            //Add Heartbroken and Betrayed trait to Recipient.
            recipient.AddTrait("Betrayed", actor);
            recipient.AddTrait("Heartbroken", actor);
        }
        //Recipient considers Actor as a Paramour and Actor and Target are Lovers:
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.PARAMOUR) && actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "I will make sure that [Actor Name] leaves [Target Name] soon!"
            reactions.Add(string.Format("I will make sure that {0} leaves {1} soon!", actor.name, target.name));
            //-**Recipient Effect * *: Add a Destroy Love Job to Recipient versus Actor or Target(choose at random).
            Character randomTarget = actor;
            if (Random.Range(0, 2) == 1) {
                randomTarget = target;
            }
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DESTROY_LOVE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = randomTarget },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { randomTarget } }, });
            job.SetCannotOverrideJob(true);
            //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            recipient.jobQueue.AddJobInQueue(job, false);
        }
        //Recipient considers Target as a Paramour and Actor and Target are Lovers:
        else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "I will make sure that [Target Name] leaves [Actor Name] soon!"
            reactions.Add(string.Format("I will make sure that {0} leaves {1} soon!", target.name, actor.name));
            //-**Recipient Effect * *: Add a Destroy Love Job to Recipient versus Actor or Target(choose at random).
            Character randomTarget = actor;
            if (Random.Range(0, 2) == 1) {
                randomTarget = target;
            }
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DESTROY_LOVE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = randomTarget },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { randomTarget } }, });
            job.SetCannotOverrideJob(true);
            //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            recipient.jobQueue.AddJobInQueue(job, false);
        }
        //Actor and Target are Paramours. Recipient has no positive relationship with either of them. Actor has a Lover and Recipient has a positive relationship with Actor's Lover:
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipientWithActor != RELATIONSHIP_EFFECT.POSITIVE && recipientWithTarget != RELATIONSHIP_EFFECT.POSITIVE
            && actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: "[Actor's Lover Name] must know about [Actor Name]'s affair with [Target Name]!"
            reactions.Add(string.Format("{0} must know about {1}'s affair with {2}!", actorLover.name, actor.name, target.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Actor's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actorAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData(JOB_TYPE.SHARE_INFORMATION, this)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SHARE_INFORMATION, INTERACTION_TYPE.SHARE_INFORMATION, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, false);
            }
        }
        //Actor and Target are Paramours. Recipient has no positive relationship with either of them. Target has a Lover and Recipient has a positive relationship with Target's Lover:
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipientWithActor != RELATIONSHIP_EFFECT.POSITIVE && recipientWithTarget != RELATIONSHIP_EFFECT.POSITIVE
            && targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: "[Target's Lover Name] must know about [Target Name]'s affair with [Actor Name]!"
            reactions.Add(string.Format("{0} must know about {1}'s affair with {2}!", targetLover.name, target.name, actor.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Target's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, poiTargetAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData(JOB_TYPE.SHARE_INFORMATION, this)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SHARE_INFORMATION, INTERACTION_TYPE.SHARE_INFORMATION, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, false);
            }
        }
        //Actor and Target are Paramours. Recipient has no positive relationship with either of them. Recipient is from the same faction as either one of them.
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipientWithActor != RELATIONSHIP_EFFECT.POSITIVE && recipientWithTarget != RELATIONSHIP_EFFECT.POSITIVE
            && (recipient.faction == actor.faction || recipient.faction == target.faction)) {
            //- **Recipient Response Text**: "I'm not interested in meddling in other people's private lives."
            reactions.Add("I'm not interested in meddling in other people's private lives.");
            //-**Recipient Effect * *: No effect
        }
        //Actor and Target are Paramours. Recipient considers Actor as an Enemy. Actor has a Lover and Recipient does not consider Actor's Lover an Enemy.
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY) && actorLover != null
            && !recipient.HasRelationshipOfTypeWith(actorLover, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "I can't wait to let [Actor's Lover's Name] find out about this affair."
            reactions.Add(string.Format("I can't wait to let {0} find out about this affair.", actorLover.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Actor's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actorAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData(JOB_TYPE.SHARE_INFORMATION, this)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SHARE_INFORMATION, INTERACTION_TYPE.SHARE_INFORMATION, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, false);
            }
        }
        //Actor and Target are Paramours. Recipient considers Target as an Enemy. Target has a Lover and Recipient does not consider Target's Lover an Enemy.
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY) && targetLover != null
            && !recipient.HasRelationshipOfTypeWith(targetLover, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "I can't wait to let [Target's Lover's Name] find out about this affair."
            reactions.Add(string.Format("I can't wait to let {0} find out about this affair.", targetLover.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Target's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, poiTargetAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData(JOB_TYPE.SHARE_INFORMATION, this)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SHARE_INFORMATION, INTERACTION_TYPE.SHARE_INFORMATION, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, false);
            }
        }
        return reactions;
    }
    #endregion
}
