using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDestroy : GoapAction {
    public ItemDestroy(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ITEM_DESTROY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
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
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Destroy Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 10;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region State Effects
    private void PreDestroySuccess() {
        currentState.AddLogFiller(null, poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterDestroySuccess() {
        //**After Effect 1**: Destroy target item
        poiTarget.gridTileLocation.structure.RemovePOI(poiTarget);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(null, poiTarget.name, LOG_IDENTIFIER.ITEM_1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return poiTarget.state == POI_STATE.ACTIVE;
    }
    #endregion
}
