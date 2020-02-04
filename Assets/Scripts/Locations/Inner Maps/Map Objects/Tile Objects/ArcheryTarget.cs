using System.Collections.Generic;

public class ArcheryTarget : TileObject{
    
    public ArcheryTarget() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.ARCHERY_TARGET);
        RemoveCommonAdvertisements();
    }
    public ArcheryTarget(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
