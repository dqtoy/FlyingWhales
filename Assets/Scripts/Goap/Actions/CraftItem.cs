using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftItem : GoapAction {

    public CraftItem() : base(INTERACTION_TYPE.CRAFT_ITEM) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_WOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        SPECIAL_TOKEN craftedItem;
        if (node.poiTarget is SpecialToken) {
            craftedItem = (node.poiTarget as SpecialToken).specialTokenType;
        } else {
            craftedItem = (SPECIAL_TOKEN)node.otherData[0];
        }

        log.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.ownParty.RemoveCarriedPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.ownParty.RemoveCarriedPOI();
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        SPECIAL_TOKEN craftedItem;
        if (poiTarget is SpecialToken) {
            craftedItem = (poiTarget as SpecialToken).specialTokenType;
        } else {
            craftedItem = (SPECIAL_TOKEN)otherData[0];
        }
        if (actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile) {
            ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            return carriedPile.resourceInPile >= TokenManager.Instance.itemData[craftedItem].craftCost;
        }
        return false;
        //return actor.supply >= TokenManager.Instance.itemData[craftedItem].craftCost; 
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is SpecialToken) {
                if (poiTarget.gridTileLocation == null) {
                    return false;
                }
                return (poiTarget as SpecialToken).specialTokenType.CanBeCraftedBy(actor);
            } else {
                if (actor != poiTarget) {
                    return false;
                }
                SPECIAL_TOKEN craftedItem = (SPECIAL_TOKEN)otherData[0];
                //if the crafted item enum has been set, check if the actor has the needed trait to craft it
                return craftedItem.CanBeCraftedBy(actor);
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        SPECIAL_TOKEN craftedItem;
        if (goapNode.poiTarget is SpecialToken) {
            craftedItem = (goapNode.poiTarget as SpecialToken).specialTokenType;
        } else {
            craftedItem = (SPECIAL_TOKEN)goapNode.otherData[0];
        }
        goapNode.descriptionLog.AddToFillers(null, Utilities.GetArticleForWord(craftedItem.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(craftedItem.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        SPECIAL_TOKEN craftedItem;
        if (goapNode.poiTarget is SpecialToken) {
            craftedItem = (goapNode.poiTarget as SpecialToken).specialTokenType;
            (goapNode.poiTarget as SpecialToken).SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        } else {
            craftedItem = (SPECIAL_TOKEN)goapNode.otherData[0];
            SpecialToken tool = TokenManager.Instance.CreateSpecialToken(craftedItem);
            goapNode.actor.ObtainToken(tool);
        }
        ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
        carriedPile.AdjustResourceInPile(-TokenManager.Instance.itemData[craftedItem].craftCost);
        //goapNode.actor.AdjustSupply(-TokenManager.Instance.itemData[craftedItem].craftCost);
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
