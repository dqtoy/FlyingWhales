using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftItemGoap : GoapAction {
    public SPECIAL_TOKEN craftedItem { get; private set; }

    public CraftItemGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CRAFT_ITEM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Work_Icon;
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
        base.PerformActualAction();
        SetState("Craft Success");
    }
    protected override int GetCost() {
        return 2;
    }
    protected override void CreateThoughtBubbleLog() {
        base.CreateThoughtBubbleLog();
        thoughtBubbleLog.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        thoughtBubbleMovingLog.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        planLog.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);

        thoughtBubbleLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.ITEM_1);
        thoughtBubbleMovingLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        planLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Preconditions
    private bool HasSupply() {
        return actor.supply >= ItemManager.Instance.itemData[craftedItem].craftCost;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor.GetToken(poiTarget as SpecialToken) == null;
    }
    #endregion

    #region State Effects
    private void PreCraftSuccess() {
        currentState.AddLogFiller(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.ITEM_1);

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
