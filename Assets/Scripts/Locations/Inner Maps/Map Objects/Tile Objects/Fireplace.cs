using System.Collections.Generic;

public class Fireplace : TileObject{
    public Fireplace() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.FIREPLACE);
        RemoveCommonAdvertisments();
    }
    public Fireplace(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
