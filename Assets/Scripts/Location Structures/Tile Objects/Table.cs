using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Table : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }

    public Table(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_DWELLING_TABLE, INTERACTION_TYPE.DRINK, INTERACTION_TYPE.TABLE_REMOVE_POISON, INTERACTION_TYPE.TABLE_POISON, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.TABLE);
    }

    public override string ToString() {
        return "Table " + id.ToString();
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }
}
