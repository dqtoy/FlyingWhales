using System.Collections.Generic;

public class TableConjuring : TileObject{
    public TableConjuring() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_CONJURING);
        RemoveCommonAdvertisements();
    }
    public TableConjuring(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
