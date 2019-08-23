using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftFurniture : GoapAction {
    public LocationGridTile targetSpot { get; private set; }
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }
    private LocationStructure _targetStructure;
    public TILE_OBJECT_TYPE furnitureToCreate { get; private set; }
    private bool hasSetOtherData;

    public CraftFurniture(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CRAFT_FURNITURE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Work_Icon;
        SetShowIntelNotification(false);
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (hasSetOtherData) {
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost, targetPOI = actor }, () => HasSupply(TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost));
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = craftedItem.ToString(), targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (targetSpot.objHere == null) {
            SetState("Craft Success");
        } else {
            SetState("Craft Fail");
        }
    }
    protected override int GetCost() {
        return 2;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return targetSpot;
    }
    protected override void CreateThoughtBubbleLog() {
        base.CreateThoughtBubbleLog();
        if (hasSetOtherData) {
            thoughtBubbleLog.AddToFillers(null, Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);
            thoughtBubbleMovingLog.AddToFillers(null, Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);

            thoughtBubbleLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
            thoughtBubbleMovingLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
        }
    }
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 2 && otherData[0] is LocationGridTile && otherData[1] is FURNITURE_TYPE) {
            SetTargetSpot(otherData[0] as LocationGridTile);
            SetFurnitureToCraft((FURNITURE_TYPE) otherData[1]);
            hasSetOtherData = true;
            preconditions.Clear();
            expectedEffects.Clear();
            ConstructPreconditionsAndEffects();
            CreateThoughtBubbleLog();
            SetTargetStructure();
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (actor != poiTarget) {
            return false;
        }
        //return true;
        if (hasSetOtherData) {
            //if the crafted item enum has been set, check if the actor has the needed trait to craft it
            return furnitureToCreate.CanBeCraftedBy(actor);
        } else {
            //if the creafted enum has NOT been set, always allow, since we know that the character has the ability to craft furniture 
            //because craft furniture action is only added to characters with traits that allow crafting
            return true;
        }

    }
    #endregion

    #region State Effects
    private void PreCraftSuccess() {
        currentState.AddLogFiller(null, Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(null, Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    private void AfterCraftSuccess() {
        actor.AdjustSupply(-TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost);
        targetSpot.structure.AddTileObject(furnitureToCreate, targetSpot, false);
    }
    #endregion

    public void SetFurnitureToCraft(FURNITURE_TYPE type) {
        furnitureToCreate = type.ConvertFurnitureToTileObject();
        
    }
    private void SetTargetSpot(LocationGridTile spot) {
        targetSpot = spot;
        _targetStructure = targetSpot.structure;
    }
}

public class CraftFurnitureData : GoapActionData {
    public CraftFurnitureData() : base(INTERACTION_TYPE.CRAFT_FURNITURE) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            return false;
        }
        if(otherData == null) {
            return true;
        }
        TILE_OBJECT_TYPE furnitureToCreate = (TILE_OBJECT_TYPE) otherData[0];
        //if the creafted enum has NOT been set, always allow, since we know that the character has the ability to craft furniture 
        //because craft furniture action is only added to characters with traits that allow crafting
        return furnitureToCreate.CanBeCraftedBy(actor);
    }
}
