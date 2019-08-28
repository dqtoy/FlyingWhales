using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWell : TileObject {

    public WaterWell(LocationStructure location) {
        this.structureLocation = location;
        if (location.structureType != STRUCTURE_TYPE.POND) {
            poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.WELL_JUMP };
        } else {
            poiGoapActions = new List<INTERACTION_TYPE>();
        }
        
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        RemoveTrait("Flammable");
        Wet wet = new Wet();
        wet.daysDuration = 0;
        AddTrait(wet);
    }
    public override string ToString() {
        return "Well " + id.ToString();
    }
}
