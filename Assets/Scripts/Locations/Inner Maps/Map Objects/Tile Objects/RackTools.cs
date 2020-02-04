using System.Collections.Generic;

public class RackTools : TileObject{
    public RackTools() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.RACK_TOOLS);
        RemoveCommonAdvertisements();
    }
    public RackTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
