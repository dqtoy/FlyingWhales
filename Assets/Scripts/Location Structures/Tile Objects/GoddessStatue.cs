using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoddessStatue : TileObject {

    public GoddessStatue(LocationStructure location) {
        SetStructureLocation(location);
        //if (location.structureType != STRUCTURE_TYPE.POND) {
        //    poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.WELL_JUMP, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        //} else {
        //    poiGoapActions = new List<INTERACTION_TYPE>();
        //}
        advertisedActions = new List<INTERACTION_TYPE>();

        Initialize(TILE_OBJECT_TYPE.GODDESS_STATUE);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public GoddessStatue(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }

    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (state == POI_STATE.INACTIVE) {
            advertisedActions.Add(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            advertisedActions.Remove(INTERACTION_TYPE.PRAY_TILE_OBJECT);
        } else {
            advertisedActions.Remove(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            advertisedActions.Add(INTERACTION_TYPE.PRAY_TILE_OBJECT);
        }
        if (gridTileLocation != null) {
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
        }

    }
    public override void SetGridTileLocation(LocationGridTile tile) {
        base.SetGridTileLocation(tile);
        if (tile != null) {
            SetPOIState(POI_STATE.INACTIVE);
        }
    }
    public override string ToString() {
        return "Goddess Statue " + id.ToString();
    }
}
