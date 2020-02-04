using System.Collections.Generic;

public class ShelfSwords : TileObject{
    public ShelfSwords() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.SHELF_SWORDS);
        RemoveCommonAdvertisements();
    }
    public ShelfSwords(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
