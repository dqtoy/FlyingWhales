using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class WaterWell : TileObject {

    public WaterWell(LocationStructure location) {
        SetStructureLocation(location);
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        traitContainer.RemoveTrait(this, "Flammable");
        Wet wet = new Wet();
        wet.daysDuration = 0;
        traitContainer.AddTrait(this, wet);
    }
    public WaterWell(SaveDataTileObject data) {
        Initialize(data);
    }
    public override string ToString() {
        return "Well " + id.ToString();
    }
    public override void SetStructureLocation(LocationStructure structure) {
        base.SetStructureLocation(structure);
        if (structure.structureType != STRUCTURE_TYPE.POND) {
            advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.WELL_JUMP, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        } else {
            advertisedActions = new List<INTERACTION_TYPE>();
        }
    }
}
