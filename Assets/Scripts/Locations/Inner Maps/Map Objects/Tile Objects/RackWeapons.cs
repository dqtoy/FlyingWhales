using System.Collections.Generic;

public class RackWeapons : TileObject{
    public RackWeapons() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.RACK_WEAPONS);
        RemoveCommonAdvertisements();
    }
    public RackWeapons(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
