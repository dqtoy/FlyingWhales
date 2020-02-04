using System.Collections.Generic;

public class TempleAltar : TileObject{
    public TempleAltar() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TEMPLE_ALTAR);
        RemoveCommonAdvertisements();
    }
    public TempleAltar(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
