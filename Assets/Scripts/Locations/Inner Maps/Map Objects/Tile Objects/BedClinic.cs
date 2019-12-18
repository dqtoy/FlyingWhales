﻿using System.Collections.Generic;

public class BedClinic : TileObject{
    public BedClinic() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.BED_CLINIC);
    }
    public BedClinic(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}