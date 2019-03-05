using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItemGoap : GoapAction {
    public PickItemGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PICK_ITEM, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget.name, targetPOI = actor });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            SpecialToken tool = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.TOOL);
            actor.ObtainToken(tool);
            return true;
        }
        return false;
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return !actor.isHoldingItem && poiTarget.gridTileLocation != null;
    }
    #endregion
}
