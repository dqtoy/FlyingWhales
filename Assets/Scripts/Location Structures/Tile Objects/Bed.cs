using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bed : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }

    public Bed(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SLEEP, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.BED);
    }

    public override string ToString() {
        return "Bed " + id.ToString();
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }
}
