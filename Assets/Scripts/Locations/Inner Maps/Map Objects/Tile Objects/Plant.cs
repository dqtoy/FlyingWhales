using System.Collections.Generic;

public class Plant : TileObject{
    public Plant() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.PLANT);
        RemoveCommonAdvertisments();
    }
    public Plant(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
