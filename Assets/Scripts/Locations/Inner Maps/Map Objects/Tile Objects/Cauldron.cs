using System.Collections.Generic;

public class Cauldron : TileObject{
    public Cauldron() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.CAULDRON);
        RemoveCommonAdvertisments();
    }
    public Cauldron(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
