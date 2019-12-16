using System.Collections.Generic;

public class Brazier : TileObject{
    
    public Brazier() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.BRAZIER);
        RemoveCommonAdvertisments();
    }
    public Brazier(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
