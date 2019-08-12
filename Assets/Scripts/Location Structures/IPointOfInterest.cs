using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointOfInterest : ITraitable{

    POINT_OF_INTEREST_TYPE poiType { get; }
    POI_STATE state { get; }
    Area specificLocation { get; }
    List<INTERACTION_TYPE> poiGoapActions { get; }
    //List<Trait> normalTraits { get; }
    Faction factionOwner { get; }
    POICollisionTrigger collisionTrigger { get; } //Each poi must only hav 1 at a time.
    bool isDisabledByPlayer { get; }

    void SetGridTileLocation(LocationGridTile tile);
    List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions);
    LocationGridTile GetNearestUnoccupiedTileFromThis();
    void AddAdvertisedAction(INTERACTION_TYPE actionType);
    void RemoveAdvertisedAction(INTERACTION_TYPE actionType);

    void SetPOIState(POI_STATE state);
    void SetIsDisabledByPlayer(bool state);
    bool IsAvailable();

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
