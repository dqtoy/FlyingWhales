using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatAnimal : GoapAction {
    public EatAnimal(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_SMALL_ANIMAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            if(poiTarget.state != POI_STATE.INACTIVE) {
                SetState("Eat Success");
            } else {
                SetState("Eat Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        if (actor.GetTrait("Carnivore") != null) {
            return 3;
        } else {
            return 12;
        }
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.ToString(), LOG_IDENTIFIER.LANDMARK_1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AddTrait("Eating");
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(5);
    }
    private void AfterEatSuccess() {
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    private void PreEatFail() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.ToString(), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.ToString(), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget.state != POI_STATE.INACTIVE;
    }
    #endregion
}
