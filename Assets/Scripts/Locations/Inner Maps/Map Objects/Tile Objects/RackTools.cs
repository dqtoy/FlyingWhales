using System.Collections.Generic;

public class RackTools : TileObject{
    public RackTools() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.RACK_TOOLS);
        RemoveCommonAdvertisments();
    }
    public RackTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
