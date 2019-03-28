using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyPile : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }
    public int suppliesInPile { get; private set; }

    public SupplyPile(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_SUPPLY, INTERACTION_TYPE.DROP_SUPPLY };
        Initialize(TILE_OBJECT_TYPE.SUPPLY_PILE);
        SetSuppliesInPile(1000);
    }

    public void SetSuppliesInPile(int amount) {
        suppliesInPile = amount;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void AdjustSuppliesInPile(int adjustment) {
        suppliesInPile += adjustment;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
        //if (suppliesInPile == 0) {
            //LocationGridTile loc = gridTileLocation;
            //location.RemovePOI(this);
            //SetGridTileLocation(loc); //so that it can still be targetted by aware characters.
        //}
    }

    public bool HasSupply() {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return suppliesInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Supply Pile " + id.ToString();
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        if (tile != null) {
            tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        }
        base.SetGridTileLocation(tile);
    }
}
