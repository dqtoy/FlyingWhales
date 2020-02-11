using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : TileObject {

    public Tool() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TOOL);
        RemoveCommonAdvertisements();
    }
    public Tool(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
