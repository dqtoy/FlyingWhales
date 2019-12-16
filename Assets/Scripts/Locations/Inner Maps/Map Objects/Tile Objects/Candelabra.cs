using System.Collections.Generic;

public class Candelabra : TileObject{
    
    public Candelabra() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.CANDELABRA);
        RemoveCommonAdvertisments();
    }
    public Candelabra(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
