using System.Collections.Generic;

public class RackStaves : TileObject{
    public RackStaves() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.RACK_STAVES);
        RemoveCommonAdvertisements();
    }
    public RackStaves(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
