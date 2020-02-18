using System.Collections.Generic;

public class WaterBucket : TileObject{
    public WaterBucket() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.WATER_BUCKET);
        RemoveCommonAdvertisements();
    }
    public WaterBucket(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
        RemoveCommonAdvertisements();
    }

    public override string ToString() {
        return $"Water Bucket {id.ToString()}";
    }
}
