using System.Collections.Generic;

public class Crate : TileObject{
    public Crate() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.CRATE);
        RemoveCommonAdvertisments();
    }
    public Crate(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
