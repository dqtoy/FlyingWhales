using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guitar : TileObject {

    public Guitar(LocationStructure location) {
        this.structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(TILE_OBJECT_TYPE.GUITAR);
    }
    public override string ToString() {
        return "Guitar " + id.ToString();
    }
}
