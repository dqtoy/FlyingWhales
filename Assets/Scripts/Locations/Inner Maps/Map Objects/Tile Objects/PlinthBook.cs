using System.Collections.Generic;

public class PlinthBook : TileObject{
    public PlinthBook() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.PLINTH_BOOK);
        RemoveCommonAdvertisments();
    }
    public PlinthBook(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }    
}
