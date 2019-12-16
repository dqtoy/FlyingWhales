using System.Collections.Generic;

public class Shelf : TileObject{
    public Shelf() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.SHELF);
        RemoveCommonAdvertisments();
    }
    public Shelf(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
