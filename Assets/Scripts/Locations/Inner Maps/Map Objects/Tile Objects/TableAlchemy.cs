using System.Collections.Generic;

public class TableAlchemy : TileObject{
    public TableAlchemy() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_ALCHEMY);
        RemoveCommonAdvertisments();
    }
    public TableAlchemy(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
