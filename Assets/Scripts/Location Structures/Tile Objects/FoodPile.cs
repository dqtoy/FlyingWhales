using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodPile : TileObject {
    public int foodInPile { get; private set; }

    public FoodPile(LocationStructure location) {
        SetStructureLocation(location);
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_FOOD, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR_TILE_OBJECT, INTERACTION_TYPE.DESTROY_FOOD };
        Initialize(TILE_OBJECT_TYPE.FOOD_PILE);
        SetFoodInPile(2000); //
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public FoodPile(SaveDataTileObject data) {
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_FOOD, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR_TILE_OBJECT, INTERACTION_TYPE.DESTROY_FOOD };
        Initialize(data);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
    }
    #endregion

    public void SetFoodInPile(int amount) {
        foodInPile = amount;
        foodInPile = Mathf.Max(0, foodInPile);
    }

    public void AdjustFoodInPile(int adjustment) {
        foodInPile += adjustment;
        foodInPile = Mathf.Max(0, foodInPile);
        if (adjustment < 0) {
            Messenger.Broadcast(Signals.FOOD_IN_PILE_REDUCED, this);
        }
    }
    public bool HasSupply() {
        if (structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return foodInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Food Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
}

public class SaveDataFoodPile : SaveDataTileObject {
    public int foodInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        FoodPile obj = tileObject as FoodPile;
        foodInPile = obj.foodInPile;
    }

    public override TileObject Load() {
        FoodPile obj = base.Load() as FoodPile;
        obj.SetFoodInPile(foodInPile);
        return obj;
    }
}