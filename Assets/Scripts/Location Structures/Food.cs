using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : IPointOfInterest {

    public LocationStructure location { get; private set; }
    public FOOD foodType { get; private set; }
    public string foodName { get; private set; }

    private LocationGridTile tile;

    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.FOOD; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    #endregion

    public Food(LocationStructure location, FOOD foodType) {
        this.location = location;
        this.foodType = foodType;
        this.foodName = Utilities.NormalizeStringUpperCaseFirstLetters(this.foodType.ToString());
    }

    public override string ToString() {
        return foodName;
    }

    #region Interface
    public void SetGridTileLocation(LocationGridTile tile) {
        if (tile != null) {
            location.AdjustFoodCount(foodType, 1);
        } else {
            location.AdjustFoodCount(foodType, -1);
        }
        this.tile = tile;
    }
    #endregion
}
