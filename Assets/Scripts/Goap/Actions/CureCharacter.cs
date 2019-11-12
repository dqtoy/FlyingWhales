using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CureCharacter : GoapAction {

    public CureCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CURE_CHARACTER, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
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
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.HEALING_POTION.ToString(), targetPOI = actor }, HasItemInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Sick", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Infected", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Plagued", targetPOI = poiTarget });

    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Cure Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 12;
    }
    #endregion

    #region State Effects
    public void PreCureSuccess() {
        ////**Pre Effect 1**: Prevent movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);
        currentState.SetIntelReaction(CureSuccessReactions);
    }
    public void AfterCureSuccess() {
        //**After Effect 1**: Reduce target's Sick trait
        //if(parentPlan.job != null) {
        //    parentPlan.job.SetCannotCancelJob(true);
        //}
        //SetCannotCancelAction(true);
        RemoveTraitFrom(poiTarget, "Sick", actor);
        RemoveTraitFrom(poiTarget, "Plagued", actor);
        RemoveTraitFrom(poiTarget, "Infected", actor);
        //**After Effect 2**: Remove Healing Potion from Actor's Inventory
        actor.ConsumeToken(actor.GetToken(SPECIAL_TOKEN.HEALING_POTION));
        //**After Effect 3**: Allow movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);
    }
    #endregion

    #region Preconditions
    private bool HasItemInInventory() {
        return actor.HasTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        //return true;
    }
    #endregion

    #region Intel Reactions
    private List<string> CureSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
                else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("I am grateful that {0} helped {1}.", actor.name, targetCharacter.name));
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
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

public class CureCharacterData : GoapActionData {
    public CureCharacterData() : base(INTERACTION_TYPE.CURE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        requirementAction = Requirement;
    }
    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.GetNormalTrait("Sick", "Infected", "Plagued") != null;
    }
}
