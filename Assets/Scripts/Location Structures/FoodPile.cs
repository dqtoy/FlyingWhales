using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodPile : TileObject {
    public LocationStructure location { get; private set; }
    public int foodInPile { get; private set; }

    public FoodPile(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_FOOD, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(TILE_OBJECT_TYPE.FOOD_PILE);
        SetFoodInPile(2000);
        RemoveTrait("Flammable");
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
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
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
