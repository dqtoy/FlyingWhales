using System.Collections.Generic;

public class Rock : TileObject{
    public Rock() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.ROCK);
        RemoveCommonAdvertisements();
    }
    public Rock(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
