using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectDestroy : GoapAction {
    public TileObjectDestroy(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Destroy Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 10;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region State Effects
    private void PreDestroySuccess() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterDestroySuccess() {
        //**After Effect 1**: Destroy target tile object
        poiTarget.gridTileLocation.structure.RemovePOI(poiTarget, actor);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion
}
