using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ore : TileObject {
    public int yield { get; private set; }

    private const int Supply_Per_Mine = 50;

    public Ore(LocationStructure location) {
        this.structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE_GOAP, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.ORE);
        yield = Random.Range(15, 36);
    }

    #region Overrides
    public override string ToString() {
        return "Ore " + id.ToString();
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null) {
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
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
}
