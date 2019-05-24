using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tree : TileObject {
    public int yield { get; private set; }

    private const int Supply_Per_Mine = 25;

    public Tree(LocationStructure location) {
        this.structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.TREE);
        yield = Random.Range(15, 36);
    }

    public override string ToString() {
        return "Tree " + id.ToString();
    }

    public int GetSupplyPerMine() {
        if (yield < Supply_Per_Mine) {
            return yield;
        }
        return Supply_Per_Mine;
    }
    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0) {
            LocationGridTile loc = gridTileLocation;
            structureLocation.RemovePOI(this);
            SetGridTileLocation(loc); //so that it can still be targetted by aware characters.
        }
    }
}
