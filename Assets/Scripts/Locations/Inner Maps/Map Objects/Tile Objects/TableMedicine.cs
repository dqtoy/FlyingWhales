using System.Collections.Generic;

public class TableMedicine : TileObject {
 
    public TableMedicine() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_MEDICINE);
        RemoveCommonAdvertisements();
    }
    public TableMedicine(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
