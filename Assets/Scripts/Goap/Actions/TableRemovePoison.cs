using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableRemovePoison : GoapAction {
    public TableRemovePoison(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TABLE_REMOVE_POISON, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Remove Poison";
        actionIconString = GoapActionStateDB.Social_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation != null && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            if (poiTarget.GetTrait("Poisoned") != null) {
                SetState("Remove Poison Success");
            } else {
                SetState("Remove Poison Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Remove Poison Fail");
    }
    #endregion

    #region State Effects
    public void PreRemovePoisonSuccess() {
        //**Effect 1**: Remove Poisoned Trait from target table
        poiTarget.RemoveTrait("Poisoned");
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        //**Advertiser**: All Tables with Poisoned trait
        return poiTarget.GetTrait("Poisoned") != null;
    }
    #endregion
}
