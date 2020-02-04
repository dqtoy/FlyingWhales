using System.Collections.Generic;

public class Chains : TileObject {
    public Chains() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.CHAINS);
        RemoveCommonAdvertisements();
    }
    public Chains(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
