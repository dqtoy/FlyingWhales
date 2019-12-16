using System.Collections.Generic;

public class ShelfArmor : TileObject{
    public ShelfArmor() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.SHELF_ARMOR);
        RemoveCommonAdvertisments();
    }
    public ShelfArmor(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
