using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrainCharacter : GoapAction {

    public RestrainCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RESTRAIN_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget }, HasNonPositiveDisablerTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation))) {
            SetState("Restrain Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreRestrainSuccess() {
        //**Effect 1**: Target gains Restrained trait.
        AddTraitTo(poiTarget, "Restrained");
    }
    public void PreTargetMissing() {
        //**Effect 1**: Remove Target from Actor's Awareness list
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
