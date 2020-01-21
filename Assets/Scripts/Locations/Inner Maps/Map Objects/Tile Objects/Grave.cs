using System.Collections.Generic;

public class Grave : TileObject{
    public Grave() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.GRAVE);
        RemoveCommonAdvertisements();
    }
    public Grave(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
