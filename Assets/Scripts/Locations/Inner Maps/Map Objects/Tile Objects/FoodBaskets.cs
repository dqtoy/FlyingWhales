using System.Collections.Generic;

public class FoodBaskets : TileObject{
    public FoodBaskets() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.FOOD_BASKETS);
        RemoveCommonAdvertisements();
    }
    public FoodBaskets(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
