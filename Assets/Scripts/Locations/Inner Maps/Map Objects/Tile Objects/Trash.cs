using System.Collections.Generic;

public class Trash : TileObject{
    public Trash() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TRASH);
        RemoveCommonAdvertisements();
    }
    public Trash(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
