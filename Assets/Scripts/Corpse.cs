using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Corpse : TileObject, IPointOfInterest {
    public Character character { get; private set; }
    public LocationStructure location { get; private set; }

    public Corpse(Character character, LocationStructure structure) {
        this.character = character;
        location = structure;
        Initialize(TILE_OBJECT_TYPE.CORPSE);
    }

    public override string ToString() {
        return "Corpse of " + character.name;
    }
}