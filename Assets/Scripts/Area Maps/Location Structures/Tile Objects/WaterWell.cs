using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class WaterWell : TileObject {

    public WaterWell() {
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        traitContainer.RemoveTrait(this, "Flammable");
        Wet wet = new Wet();
        wet.ticksDuration = 0;
        traitContainer.AddTrait(this, wet);
    }
    public WaterWell(SaveDataTileObject data) {
        Initialize(data);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (structureLocation.structureType != STRUCTURE_TYPE.POND) {
            advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.WELL_JUMP, INTERACTION_TYPE.REPAIR };
        } else {
            advertisedActions = new List<INTERACTION_TYPE>();
        }
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND;
    }
    public override string ToString() {
        return "Well " + id.ToString();
    }
}
