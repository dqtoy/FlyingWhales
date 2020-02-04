using System.Collections.Generic;

public class Bandages : TileObject {
    public Bandages() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.BANDAGES);
        RemoveCommonAdvertisements();
    }
    public Bandages(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
