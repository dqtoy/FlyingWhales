using System.Collections.Generic;

public class Barrel : TileObject{
    public Barrel() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.BARREL);
        RemoveCommonAdvertisements();
    }
    public Barrel(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
