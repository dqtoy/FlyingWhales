using System.Collections.Generic;

public class RegionTileObject : TileObject {
    
    public RegionTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.REGION_TILE_OBJECT);
    }
    public RegionTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return "Region Tile Object " + id.ToString();
    }
    #endregion
}
