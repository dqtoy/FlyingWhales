using System.Collections.Generic;

public class Rug : TileObject{
     public Rug() {
            advertisedActions = new List<INTERACTION_TYPE>();
            Initialize(TILE_OBJECT_TYPE.RUG);
            RemoveCommonAdvertisments();
     }
     public Rug(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
     }
        
}
