using System.Collections.Generic;

public class TableArmor : TileObject{
    public TableArmor() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_ARMOR);
        RemoveCommonAdvertisments();
    }
    public TableArmor(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
