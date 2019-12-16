using System.Collections.Generic;

public class SmithingForge : TileObject{
    public SmithingForge() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.SMITHING_FORGE);
        RemoveCommonAdvertisments();
    }
    public SmithingForge(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
