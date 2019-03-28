using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSupply : GoapAction {
    public DropSupply(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_SUPPLY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = actor.supply, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        SetState("Drop Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor.homeArea == poiTarget.gridTileLocation.structure.location && actor.supply > actor.role.reservedSupply;
    }
    #endregion

    #region State Effects
    private void PreDropSuccess() {
        int givenSupply = actor.supply - actor.role.reservedSupply;
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, givenSupply.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDropSuccess() {
        SupplyPile supplyPile = poiTarget as SupplyPile;
        int givenSupply = actor.supply - actor.role.reservedSupply;
        actor.AdjustSupply(-givenSupply);
        supplyPile.AdjustSuppliesInPile(givenSupply);
    }
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}
