using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class FirstAidCharacter : GoapAction {

    public FirstAidCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FIRST_AID_CHARACTER, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.HEALING_POTION.ToString(), targetPOI = actor }, () => actor.HasTokenInInventory(SPECIAL_TOKEN.HEALING_POTION));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Injured", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("First Aid Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 12;
    }
    #endregion

    #region State Effects
    public void PreFirstAidSuccess() {
        //**Pre Effect 1**: Prevent movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);
        currentState.SetIntelReaction(FirstAidSuccessReactions);
    }
    public void AfterFirstAidSuccess() {
        //**After Effect 1**: Remove target's Injured and Unconscious trait
        //if (parentPlan.job != null) {
        //    parentPlan.job.SetCannotCancelJob(true);
        //}
        //SetCannotCancelAction(true);
        RemoveTraitFrom(poiTarget, "Injured", actor);
        RemoveTraitFrom(poiTarget, "Unconscious", actor);
        //**After Effect 2**: Reduce character's Supply by 10
        //actor.AdjustSupply(-10);
        if (actor.HasTokenInInventory(SPECIAL_TOKEN.HEALING_POTION)) {
            actor.ConsumeToken(actor.GetToken(SPECIAL_TOKEN.HEALING_POTION));
        } else {
            //the actor does not have a tool, log for now
            Debug.LogWarning(actor.name + " does not have a tool for removing poison! Poison was still removed, but thought you should know.");
        }
        //**After Effect 3**: Allow movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);
    }
    #endregion

    #region Intel Reactions
    private List<string> FirstAidSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("I know what I did.");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    reactions.Add(string.Format("I am grateful for {0}'s help.", actor.name));
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("I am grateful that {0} helped {1}.", actor.name, targetCharacter.name));
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("{0} is such a chore.", targetCharacter.name));
                }
                //- Recipient Has No Relationship with Target
                else {
                    reactions.Add(string.Format("That was nice of {0}.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class FirstAidCharacterData : GoapActionData {
    public FirstAidCharacterData() : base(INTERACTION_TYPE.FIRST_AID_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        requirementAction = Requirement;
    }
    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.GetNormalTrait("Injured", "Unconscious") != null;
    }
}