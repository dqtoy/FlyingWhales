using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeObject : TileObject {
    public int yield { get; private set; }

    private const int Supply_Per_Mine = 25;

    public TreeObject(LocationStructure location) {
        SetStructureLocation(location);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(TILE_OBJECT_TYPE.TREE_OBJECT);
        SetYield(Random.Range(15, 36));
    }
    public TreeObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(data);
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
    public void SetYield(int amount) {
        yield = amount;
    }
}

public class SaveDataTreeObject: SaveDataTileObject {
    public int yield;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        TreeObject obj = tileObject as TreeObject;
        yield = obj.yield;
    }

    public override TileObject Load() {
        TreeObject obj = base.Load() as TreeObject;
        obj.SetYield(yield);
        return obj;
    }
}