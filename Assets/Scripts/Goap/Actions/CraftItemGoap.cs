using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftItemGoap : GoapAction {
    public SPECIAL_TOKEN craftedItem { get; private set; }

    public CraftItemGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CRAFT_ITEM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = ItemManager.Instance.itemData[craftedItem].craftCost, targetPOI = actor }, HasSupply);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = craftedItem, targetPOI = actor });
    }
    public override void PerformActualAction() {
        SetState("Craft Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 2;
    }
    #endregion

    #region Preconditions
    private bool HasSupply() {
        return actor.supply >= ItemManager.Instance.itemData[craftedItem].craftCost;
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
        actor.AdjustSupply(-ItemManager.Instance.itemData[craftedItem].craftCost);
    }
    private void AfterCraftSuccess() {
        SpecialToken tool = TokenManager.Instance.CreateSpecialToken(craftedItem);
        actor.ObtainToken(tool);
    }
    #endregion

    public void SetCraftedItem(SPECIAL_TOKEN item) {
        craftedItem = item;
    }
}
