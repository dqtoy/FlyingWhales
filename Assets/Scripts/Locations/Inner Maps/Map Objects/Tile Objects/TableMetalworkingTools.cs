using System.Collections.Generic;

public class TableMetalworkingTools : TileObject{
    public TableMetalworkingTools() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TABLE_METALWORKING_TOOLS);
        RemoveCommonAdvertisements();
    }
    public TableMetalworkingTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
