using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoddessStatue : TileObject {

    public GoddessStatue() {
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
            AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            RemoveAdvertisedAction(INTERACTION_TYPE.PRAY_TILE_OBJECT);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            AddAdvertisedAction(INTERACTION_TYPE.PRAY_TILE_OBJECT);
        }
        if (gridTileLocation != null) {
            areaMapVisual.UpdateTileObjectVisual(this); //update visual based on state
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
