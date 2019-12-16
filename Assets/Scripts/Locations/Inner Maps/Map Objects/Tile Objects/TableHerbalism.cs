using System.Collections.Generic;

public class TableHerbalism : TileObject{
    public TableHerbalism() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_HERBALISM);
        RemoveCommonAdvertisments();
    }
    public TableHerbalism(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
