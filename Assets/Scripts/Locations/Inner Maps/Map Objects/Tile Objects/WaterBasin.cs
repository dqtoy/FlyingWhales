using System.Collections.Generic;

public class WaterBasin : TileObject{
    
    public WaterBasin() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.WATER_BASIN);
        RemoveCommonAdvertisments();
    }
    public WaterBasin(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
