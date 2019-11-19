using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftItem : GoapAction {

    public CraftItem() : base(INTERACTION_TYPE.CRAFT_ITEM) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_WOOD, target = GOAP_EFFECT_TARGET.ACTOR }, HasSupply);
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = craftedItem.ToString(), target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 10;
    }
    public override void AddFillersToLog(Log log, Character actor, IPointOfInterest poiTarget, object[] otherData, LocationStructure targetStructure) {
        base.AddFillersToLog(log, actor, poiTarget, otherData, targetStructure);
        SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN)otherData[0];
        log.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN)otherData[0];
        return actor.supply >= TokenManager.Instance.itemData[craftedItem].craftCost;
       
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor != poiTarget) {
                return false;
            }
            SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN)otherData[0];
            //if the crafted item enum has been set, check if the actor has the needed trait to craft it
            return craftedItem.CanBeCraftedBy(actor);
   
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreCraftSuccess(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN)goapNode.otherData[0];
        goapNode.descriptionLog.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.ITEM_1);

        goapNode.actor.AdjustSupply(-TokenManager.Instance.itemData[craftedItem].craftCost);
    }
    private void AfterCraftSuccess(ActualGoapNode goapNode) {
        SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN)goapNode.otherData[0];
        SpecialToken tool = TokenManager.Instance.CreateSpecialToken(craftedItem);
        goapNode.actor.ObtainToken(tool);
    }
    #endregion
}

public class CraftItemData : GoapActionData {
    public CraftItemData() : base(INTERACTION_TYPE.CRAFT_ITEM) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            return false;
        }
        if (otherData == null) {
            return true;
        }
        if (otherData.Length == 1 && otherData[0] is SPECIAL_TOKEN) {
            SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN) otherData[0];
            //if the creafted enum has NOT been set, always allow, since we know that the character has the ability to craft an item, 
            //because craft item action is only added to characters with traits that allow crafting
            return craftedItem.CanBeCraftedBy(actor);
        }
        return false;
    }
}
