using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Desk : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }

    public Desk(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.DESK);
    }

    public override string ToString() {
        return "Desk " + id.ToString();
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
    }
}