using System.Collections.Generic;

public class Wheelbarrow : TileObject{
    public Wheelbarrow() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.WHEELBARROW);
        RemoveCommonAdvertisments();
    }
    public Wheelbarrow(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
