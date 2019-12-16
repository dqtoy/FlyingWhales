using System.Collections.Generic;

public class ShelfScrolls : TileObject{
    public ShelfScrolls() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.SHELF_SCROLLS);
        RemoveCommonAdvertisments();
    }
    public ShelfScrolls(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
