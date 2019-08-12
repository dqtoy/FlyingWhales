using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWell : TileObject {

    public WaterWell(LocationStructure location) {
        this.structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_WATER };
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        RemoveTrait("Flammable");
    }
    public override string ToString() {
        return "Well " + id.ToString();
    }
}
