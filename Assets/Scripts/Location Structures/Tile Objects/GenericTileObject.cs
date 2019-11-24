using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class GenericTileObject : TileObject {

    public bool hasBeenInitialized { get; private set; }

    public GenericTileObject(LocationStructure location) {
        advertisedActions = new List<INTERACTION_TYPE>();
    }
    public GenericTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
    protected override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this as TileObject, removedBy, removedFrom);
        if (hasCreatedSlots) {
            DestroyTileSlots();
        }
    }
    public override void SetGridTileLocation(LocationGridTile tile) {
        previousTile = this.tile;
        if (tile != null) {
            this.tile = tile;
        }
        //if (collisionTrigger == null) {
        //    InitializeCollisionTrigger(this); //TODO: Remove chance of this happening?
        //}
        if (tile == null) {
            DisableGameObject();
            OnRemoveTileObject(null, previousTile);
            SetPOIState(POI_STATE.INACTIVE);
        } else {
            PlaceMapObjectAt(tile);
            OnPlaceObjectAtTile(tile);
            SetPOIState(POI_STATE.ACTIVE);
        }
    }
    public override void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.tile;
        DisableGameObject();
        OnRemoveTileObject(removedBy, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override bool IsValidCombatTarget() {
        return false;
    }
    public override string ToString() {
        return "Generic Obj at tile " + gridTileLocation?.ToString();
    }

    public void ManualInitialize(LocationStructure location, LocationGridTile tile) {
        hasBeenInitialized = true;
        SetStructureLocation(location);
        Initialize(TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT);
        SetGridTileLocation(tile);
        DisableGameObject();

        switch (gridTileLocation.groundType) {
            case LocationGridTile.Ground_Type.Grass:
            case LocationGridTile.Ground_Type.Wood:
                    traitContainer.AddTrait(this, "Flammable");
                break;
            default:
                traitContainer.RemoveTrait(this, "Flammable");
                break;
        }
    }
}