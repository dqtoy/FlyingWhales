using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guitar : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }

    private LocationGridTile tile;
    private POI_STATE _state;

    public Guitar(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.GUITAR);
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }

    public override string ToString() {
        return "Guitar " + id.ToString();
    }
}
