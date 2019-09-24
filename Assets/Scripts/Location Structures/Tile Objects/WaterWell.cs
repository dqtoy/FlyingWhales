using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWell : TileObject {

    public WaterWell(LocationStructure location) {
        SetStructureLocation(location);
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        RemoveTrait("Flammable");
        Wet wet = new Wet();
        wet.daysDuration = 0;
        AddTrait(wet);
    }
    public WaterWell(SaveDataTileObject data) {
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
    }
    public override string ToString() {
        return "Well " + id.ToString();
    }
    public override void SetStructureLocation(LocationStructure structure) {
        base.SetStructureLocation(structure);
        if (structure.structureType != STRUCTURE_TYPE.POND) {
            poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.WELL_JUMP, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        } else {
            poiGoapActions = new List<INTERACTION_TYPE>();
        }
    }
}
