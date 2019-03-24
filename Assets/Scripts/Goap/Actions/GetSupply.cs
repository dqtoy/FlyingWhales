using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetSupply : GoapAction {
    public GetSupply(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.GET_SUPPLY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        SupplyPile supplyPile = poiTarget as SupplyPile;
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = supplyPile.suppliesInPile, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SupplyPile supplyPile = poiTarget as SupplyPile;
            if (supplyPile.suppliesInPile > 0) {
                SetState("Take Success");
            } else {
                SetState("Take Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget is SupplyPile) {
            SupplyPile supplyPile = poiTarget as SupplyPile;
            if(supplyPile.suppliesInPile > 0 && actor.homeArea == supplyPile.gridTileLocation.structure.location && actor.supply < actor.role.reservedSupply) {
                return true; 
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreTakeSuccess() {
        SupplyPile supplyPile = poiTarget as SupplyPile;
        int takenSupply = actor.role.reservedSupply - actor.supply;
        if(supplyPile.suppliesInPile < takenSupply) {
            takenSupply = supplyPile.suppliesInPile;
        }
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, takenSupply.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterTakeSuccess() {
        SupplyPile supplyPile = poiTarget as SupplyPile;
        int takenSupply = actor.role.reservedSupply - actor.supply;
        if (supplyPile.suppliesInPile < takenSupply) {
            takenSupply = supplyPile.suppliesInPile;
        }
        actor.AdjustSupply(takenSupply);
        supplyPile.AdjustSuppliesInPile(-takenSupply);
    }
    //private void PreTakeFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}
