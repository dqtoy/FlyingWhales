﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointOfInterest : ITraitable{

    int id { get; } //Be careful with how you handle this since this can duplicate depending on its poiType
    POINT_OF_INTEREST_TYPE poiType { get; }
    POI_STATE state { get; }
    Area specificLocation { get; }
    List<INTERACTION_TYPE> poiGoapActions { get; }
    List<JobQueueItem> allJobsTargettingThis { get; }
    List<GoapAction> targettedByAction { get; }
    //List<Trait> normalTraits { get; }
    Faction factionOwner { get; }
    POICollisionTrigger collisionTrigger { get; } //Each poi must only hav 1 at a time.
    bool isDisabledByPlayer { get; }

    void SetGridTileLocation(LocationGridTile tile);
    void AddAdvertisedAction(INTERACTION_TYPE actionType);
    void RemoveAdvertisedAction(INTERACTION_TYPE actionType);
    void AddJobTargettingThis(JobQueueItem job);
    void SetPOIState(POI_STATE state);
    void SetIsDisabledByPlayer(bool state);
    void AddTargettedByAction(GoapAction action);
    void RemoveTargettedByAction(GoapAction action);
    bool HasJobTargettingThis(JOB_TYPE jobType);
    bool IsAvailable();
    bool RemoveJobTargettingThis(JobQueueItem job);
    LocationGridTile GetNearestUnoccupiedTileFromThis();
    List<GoapAction> AdvertiseActionsToActor(Character actor, Dictionary<INTERACTION_TYPE, object[]> otherData);

    //#region Traits
    //bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true);
    //bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true);
    //bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null);
    //bool RemoveTrait(string traitName, bool triggerOnRemove = true, Character removedBy = null);
    //void RemoveTrait(List<Trait> traits);
    //List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType);
    //Trait GetNormalTrait(params string[] traitName);
    //#endregion

    #region Collision
    void InitializeCollisionTrigger();
    void PlaceCollisionTriggerAt(LocationGridTile tile);
    void DisableCollisionTrigger();
    void SetCollisionTrigger(POICollisionTrigger trigger);
    void PlaceGhostCollisionTriggerAt(LocationGridTile tile);
    #endregion
}

/// <summary>
/// Helper struct to contain data of generic POI's for saving and loading.
/// Usage Example:
///  - When a class has a list of poi's that it needs to save/load use this. (List<IPointOfInterest>)
/// </summary>
[System.Serializable]
public class POIData {
    public int poiID;
    public int areaID; //area location
    public POINT_OF_INTEREST_TYPE poiType;
    public TILE_OBJECT_TYPE tileObjectType; //The type of tile object that this is, should only be used if poi type is TILE_OBJECT
    public SPECIAL_TOKEN specialTokenType; //The type of item that this is, should only be used if poi type is ITEM

    public Vector3 genericTileObjectPlace; //used for generic tile objects, use this instead of id. NOTE: Generic Tile objects must ALWAYS have an areaID

    public POIData(IPointOfInterest poi) {
        poiID = poi.id;
        if (poi.gridTileLocation == null) {
            areaID = -1;
        } else {
            areaID = poi.gridTileLocation.parentAreaMap.area.id;
        }
        poiType = poi.poiType;
        tileObjectType = TILE_OBJECT_TYPE.NONE;
        specialTokenType = default(SPECIAL_TOKEN);
        genericTileObjectPlace = Vector3.zero;

        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            tileObjectType = (poi as TileObject).tileObjectType;
            if (tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                genericTileObjectPlace = poi.gridTileLocation.localPlace;
            }
        } else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            specialTokenType = (poi as SpecialToken).specialTokenType;
        }
    }

    public override string ToString() {
        string name = poiType.ToString() + " " + poiID + ".";
        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            name += " Tile Object Type: " + tileObjectType.ToString();
        } else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            name += " Item Type: " + specialTokenType.ToString();
        }
        return name;
    }
}