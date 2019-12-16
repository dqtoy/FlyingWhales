using System.Collections.Generic;

public class Bandages : TileObject {
    public Bandages() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.BANDAGES);
        RemoveCommonAdvertisments();
    }
    public Bandages(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
