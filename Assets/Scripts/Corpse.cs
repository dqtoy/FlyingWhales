using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse: IPointOfInterest {

    public Character character { get; private set; }
    public LocationStructure location { get; private set; }

    public POINT_OF_INTEREST_TYPE poiType { get { return POINT_OF_INTEREST_TYPE.CORPSE; } }

    private LocationGridTile _gridTileLocation;
    public LocationGridTile gridTileLocation {
        get { return _gridTileLocation; }
    }

    public Corpse(Character character, LocationStructure structure) {
        this.character = character;
        location = structure;
    }

    public void SetGridTileLocation(LocationGridTile tile) {
        _gridTileLocation = tile;
    }
    public override string ToString() {
        return "Corpse of " + character.name;
    }
}
