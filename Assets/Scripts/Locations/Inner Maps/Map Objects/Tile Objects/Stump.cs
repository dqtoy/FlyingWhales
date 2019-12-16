using System.Collections.Generic;

public class Stump : TileObject{
    public Stump() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.STUMP);
        RemoveCommonAdvertisments();
    }
    public Stump(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
