using System.Collections.Generic;

public class RackFarmingTools : TileObject{
    public RackFarmingTools() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.RACK_FARMING_TOOLS);
        RemoveCommonAdvertisments();
    }
    public RackFarmingTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
