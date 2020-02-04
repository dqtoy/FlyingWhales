using System.Collections.Generic;

public class Torch : TileObject{
    public Torch() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TORCH);
        RemoveCommonAdvertisements();
    }
    public Torch(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
