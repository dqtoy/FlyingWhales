using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct POIValueType {
    public int id;
    public POINT_OF_INTEREST_TYPE poiType;
    public TILE_OBJECT_TYPE tileObjectType;

    public POIValueType(int id, POINT_OF_INTEREST_TYPE poiType, TILE_OBJECT_TYPE tileObjectType) {
        this.id = id;
        this.poiType = poiType;
        this.tileObjectType = tileObjectType;
    }
}
