using System.Collections.Generic;

public class Statue : TileObject{
    public Statue() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.STATUE);
        RemoveCommonAdvertisements();
    }
    public Statue(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
