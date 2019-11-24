﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ore : TileObject {
    public int yield { get; private set; }

    private const int Supply_Per_Mine = 50;

    public Ore(LocationStructure location) {
        SetStructureLocation(location);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE, INTERACTION_TYPE.ASSAULT, };
        Initialize(TILE_OBJECT_TYPE.ORE);
        yield = Random.Range(15, 36);
    }
    public Ore(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE, INTERACTION_TYPE.ASSAULT, };
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return "Ore " + id.ToString();
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null) {
            areaMapGameObject.UpdateTileObjectVisual(this); //update visual based on state
        }
    }
    #endregion

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

public class SaveDataOre : SaveDataTileObject {
    public int yield;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Ore obj = tileObject as Ore;
        yield = obj.yield;
    }

    public override TileObject Load() {
        Ore obj = base.Load() as Ore;
        obj.SetYield(yield);
        return obj;
    }
}