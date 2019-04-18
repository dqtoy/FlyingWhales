using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableRemovePoison : GoapAction {
    protected override string failActionState { get { return "Remove Poison Fail"; } }

    public TableRemovePoison(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TABLE_REMOVE_POISON, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Remove Poison";
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL, targetPOI = actor }, HasItemTool);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        if (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsAdjacentTo(poiTarget)) {
            SetState("Remove Poison Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 4;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Remove Poison Fail");
    //}
    #endregion

    #region State Effects
    public void PreRemovePoisonSuccess() {
        //**Effect 1**: Remove Poisoned Trait from target table
        RemoveTraitFrom(poiTarget, "Poisoned");
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        //**Advertiser**: All Tables with Poisoned trait
        return poiTarget.GetTrait("Poisoned") != null;
    }
    #endregion

    #region Preconditions
    private bool HasItemTool() {
        return actor.isHoldingItem && actor.GetToken("Tool") != null;
    }
    #endregion
}
