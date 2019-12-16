using System.Collections.Generic;

public class Flower : TileObject{
    public Flower() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.FLOWER);
        RemoveCommonAdvertisments();
    }
    public Flower(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
