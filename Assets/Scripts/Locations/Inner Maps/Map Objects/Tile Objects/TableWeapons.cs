using System.Collections.Generic;

public class TableWeapons : TileObject{
    public TableWeapons() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_WEAPONS);
        RemoveCommonAdvertisments();
    }
    public TableWeapons(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
