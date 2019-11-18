using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftFurniture : GoapAction {
    public LocationGridTile targetSpot { get; private set; }
    public TILE_OBJECT_TYPE furnitureToCreate { get; private set; }
    private bool hasSetOtherData;

    public CraftFurniture() : base(INTERACTION_TYPE.CRAFT_FURNITURE) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Work_Icon;
        showIntelNotification = false;
        isNotificationAnIntel = false;
    }

   // #region Overrides
   // protected override void ConstructRequirement() {
   //     _requirementAction = Requirement;
   // }
   // protected override void ConstructBasePreconditionsAndEffects() {
   //     if (hasSetOtherData) {
   //         AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost, targetPOI = actor }, () => HasSupply(TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost));
   //     }
   //     //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = craftedItem.ToString(), targetPOI = actor });
   // }
   // public override void Perform(ActualGoapNode goapNode) {
   //     base.Perform(goapNode);
   //     if (targetSpot.objHere == null) {
   //         SetState("Craft Success");
   //     } else {
   //         SetState("Craft Fail");
   //     }
   // }
   // protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
   //     return 2;
   // }
   // public override LocationGridTile GetTargetLocationTile() {
   //     return targetSpot;
   // }
   // protected override void CreateThoughtBubbleLog() {
   //     base.CreateThoughtBubbleLog();
   //     if (hasSetOtherData) {
   //         thoughtBubbleLog.AddToFillers(null, Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);
   //         thoughtBubbleMovingLog.AddToFillers(null, Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);

   //         thoughtBubbleLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
   //         thoughtBubbleMovingLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
   //     }
   // }
   // public override bool InitializeOtherData(object[] otherData) {
   //     this.otherData = otherData;
   //     if (otherData.Length == 2 && otherData[0] is LocationGridTile && otherData[1] is FURNITURE_TYPE) {
   //         SetTargetSpot(otherData[0] as LocationGridTile);
   //         SetFurnitureToCraft((FURNITURE_TYPE) otherData[1]);
   //         hasSetOtherData = true;
   //         preconditions.Clear();
   //         expectedEffects.Clear();
   //         ConstructBasePreconditionsAndEffects();
   //         CreateThoughtBubbleLog();
   //         SetTargetStructure();
   //         return true;
   //     }
   //     return base.InitializeOtherData(otherData);
   // }
   // #endregion

   // #region Requirements
   //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
   //     if (actor != poiTarget) {
   //         return false;
   //     }
   //     //return true;
   //     if (hasSetOtherData) {
   //         if (targetSpot.objHere != null) {
   //             return false; //cannot create furniture here because there is already something occupying it.
   //         }
   //         //if the crafted item enum has been set, check if the actor has the needed trait to craft it
   //         return furnitureToCreate.CanBeCraftedBy(actor);
   //     } else {
   //         //if the creafted enum has NOT been set, always allow, since we know that the character has the ability to craft furniture 
   //         //because craft furniture action is only added to characters with traits that allow crafting
   //         return true;
   //     }

   // }
   // #endregion

   // #region State Effects
   // private void PreCraftSuccess() {
   //     goapNode.descriptionLog.AddToFillers(null, Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);
   //     goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
   // }
   // private void AfterCraftSuccess() {
   //     actor.AdjustSupply(-TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost);
   //     if (targetSpot.objHere == null) {
   //         targetSpot.structure.AddTileObject(furnitureToCreate, targetSpot, false);
   //     }
   // }
   // #endregion

   // public void SetFurnitureToCraft(FURNITURE_TYPE type) {
   //     furnitureToCreate = type.ConvertFurnitureToTileObject();
        
   // }
   // private void SetTargetSpot(LocationGridTile spot) {
   //     targetSpot = spot;
   //     _targetStructure = targetSpot.structure;
   // }
}

public class CraftFurnitureData : GoapActionData {
    public CraftFurnitureData() : base(INTERACTION_TYPE.CRAFT_FURNITURE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            return false;
        }
        if(otherData == null) {
            return true;
        }
        if (otherData.Length == 2 && otherData[1] is FURNITURE_TYPE && otherData[0] is LocationGridTile) {
            if ((otherData[0] as LocationGridTile).objHere != null) {
                return false; //cannot create furniture here because there is already something occupying it.
            }
            FURNITURE_TYPE furnitureToCreate = (FURNITURE_TYPE) otherData[1];
            //if the creafted enum has NOT been set, always allow, since we know that the character has the ability to craft furniture 
            //because craft furniture action is only added to characters with traits that allow crafting
            return furnitureToCreate.ConvertFurnitureToTileObject().CanBeCraftedBy(actor);
        }
        return false;
    }
}
