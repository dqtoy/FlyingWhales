using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftTool : GoapAction {
    public CraftTool(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CRAFT_TOOL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 25, targetPOI = actor }, HasSupply);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = "Tool", targetPOI = actor });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            SpecialToken tool = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.TOOL);
            actor.ObtainToken(tool);
            actor.AdjustSupply(-25);
            return true;
        }
        return false;
    }
    protected override int GetCost() {
        return 2;
    }
    #endregion

    #region Preconditions
    private bool HasSupply() {
        return actor.supply >= 25;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return !actor.isHoldingItem;
    }
    #endregion
}
