using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class GenericTileObject : TileObject {

    public bool hasBeenInitialized { get; private set; }

    public GenericTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>();
    }
    public GenericTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
    #region Override
    protected override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this as TileObject, removedBy, removedFrom);
        if (hasCreatedSlots) {
            DestroyTileSlots();
        }
    }
    public override void OnPlacePOI() {
        if (areaMapGameObject == null) {
            InitializeMapObject(this);
        }
        PlaceMapObjectAt(gridTileLocation);
        OnPlaceObjectAtTile(gridTileLocation);
        SetPOIState(POI_STATE.ACTIVE);
    }
    public override void OnDestroyPOI() {
        DisableGameObject();
        OnRemoveTileObject(null, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.gridTileLocation;
        DisableGameObject();
        OnRemoveTileObject(removedBy, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override bool IsValidCombatTarget() {
        return false;
    }
    public override void OnTileObjectGainedTrait(Trait trait) {
        base.OnTileObjectGainedTrait(trait);
        if (trait.IsTangible()) {
            EnableGameObject();
        }
    }
    public override void OnTileObjectLostTrait(Trait trait) {
        base.OnTileObjectLostTrait(trait);
        if (HasTangibleTrait() == false) {
            DisableGameObject();
        }
    }
    public override string ToString() {
        return "Generic Obj at tile " + gridTileLocation?.ToString();
    }
    #endregion

    private bool HasTangibleTrait() {
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            Trait currTrait = traitContainer.allTraits[i];
            if (currTrait.IsTangible()) {
                return true;
            }
        }
        return false;
    }


    public void ManualInitialize(LocationStructure location, LocationGridTile tile) {
        hasBeenInitialized = true;
        Initialize(TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT);
        SetGridTileLocation(tile);
        OnPlacePOI();
        DisableGameObject();
        RemoveCommonAdvertisments();

        //switch (gridTileLocation.groundType) {
        //    case LocationGridTile.Ground_Type.Grass:
        //    case LocationGridTile.Ground_Type.Wood:
        //            traitContainer.AddTrait(this, "Flammable");
        //        break;
        //    default:
        //        traitContainer.RemoveTrait(this, "Flammable");
        //        break;
        //}
    }
}