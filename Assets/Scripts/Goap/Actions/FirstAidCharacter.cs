using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstAidCharacter : GoapAction {

    public FirstAidCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FIRST_AID_CHARACTER, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Social_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 10, targetPOI = actor }, () => HasSupply(10));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        SetState("First Aid Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreFirstAidSuccess() {
        //**Pre Effect 1**: Prevent movement of Target
        (poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);

    }
    public void AfterFirstAidSuccess() {
        //**After Effect 1**: Remove target's Injured and Unconscious trait
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        RemoveTraitFrom(poiTarget, "Injured");
        RemoveTraitFrom(poiTarget, "Unconscious");
        //**After Effect 2**: Reduce character's Supply by 10
        actor.AdjustSupply(-10);
        //**After Effect 3**: Allow movement of Target
        (poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);
    }
    #endregion
}
