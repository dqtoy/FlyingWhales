using System.Collections.Generic;

public class ShelfBooks : TileObject{
    public ShelfBooks() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.SHELF_BOOKS);
        RemoveCommonAdvertisments();
    }
    public ShelfBooks(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }    
}
