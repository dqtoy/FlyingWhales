using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftItemGoap : GoapAction {
    public CraftItemGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CRAFT_ITEM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 25, targetPOI = actor }, HasSupply);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Craft Success");
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

    #region State Effects
    private void PreCraftSuccess() {
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.gridTileLocation.structure.ToString(), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustSupply(-25);
    }
    private void AfterCraftSuccess() {
        SpecialToken tool = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.TOOL);
        actor.ObtainToken(tool);
    }
    #endregion
}
