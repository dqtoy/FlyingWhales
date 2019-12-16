using System.Collections.Generic;

public class TableScrolls : TileObject{
    public TableScrolls() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_SCROLLS);
        RemoveCommonAdvertisments();
    }
    public TableScrolls(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
