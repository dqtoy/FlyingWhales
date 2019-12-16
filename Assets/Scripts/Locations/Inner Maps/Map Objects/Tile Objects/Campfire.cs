using System.Collections.Generic;

public class Campfire : TileObject{
    
    public Campfire() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.CAMPFIRE);
        RemoveCommonAdvertisments();
    }
    public Campfire(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
