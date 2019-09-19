using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTileObject : TileObject {

    public override List<Trait> normalTraits {
        get { return gridTileLocation.normalTraits; }
    }

    public GenericTileObject(LocationStructure location) {
        structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.GENERIC);
    }

    public override List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        return gridTileLocation.RemoveAllTraitsByType(traitType);
    }
    public override bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null, bool includeAlterEgo = true) {
        return gridTileLocation.RemoveTrait(trait, triggerOnRemove, removedBy);
    }
    public override bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (gridTileLocation != null) {
            return gridTileLocation.AddTrait(trait, characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            return false;
        }
        
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
        if (_collisionTrigger == null) {
            InitializeCollisionTrigger();
        }
        if (tile == null) {
            DisableCollisionTrigger();
            OnRemoveTileObject(null, previousTile);
            if (previousTile != null) {
                PlaceGhostCollisionTriggerAt(previousTile);
            }
            SetPOIState(POI_STATE.INACTIVE);
        } else {
            PlaceCollisionTriggerAt(tile);
            OnPlaceObjectAtTile(tile);
            SetPOIState(POI_STATE.ACTIVE);
        }
    }
    public override void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.tile;
        DisableCollisionTrigger();
        OnRemoveTileObject(removedBy, previousTile);
        if (previousTile != null) {
            PlaceGhostCollisionTriggerAt(previousTile);
        }
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override string ToString() {
        return "Generic Obj at tile " + gridTileLocation?.ToString();
    }
}