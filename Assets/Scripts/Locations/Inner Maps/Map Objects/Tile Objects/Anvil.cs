using System.Collections.Generic;

public class Anvil : TileObject {
    
    public Anvil() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.ANVIL);
        RemoveCommonAdvertisments();
    }
    public Anvil(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
