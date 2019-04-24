using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  TileObject : IPointOfInterest {

    public string name { get { return ToString(); } }
    public int id { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Faction factionOwner { get { return null; } }
    public List<INTERACTION_TYPE> poiGoapActions { get; protected set; }
    public Area specificLocation { get { return gridTileLocation.structure.location; } }
    protected List<Trait> _traits;
    public List<Trait> traits {
        get { return _traits; }
    }
    public List<Character> awareCharacters { get; private set; } //characters that are aware of this object (Used for checking if a ghost trigger should be destroyed)
    public List<string> actionHistory { get; private set; } //list of actions that was done to this object

    private LocationGridTile tile;
    private POI_STATE _state;
    private POICollisionTrigger _collisionTrigger;

    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.TILE_OBJECT; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public POI_STATE state {
        get { return _state; }
    }
    public POICollisionTrigger collisionTrigger {
        get { return _collisionTrigger; }
    }
    #endregion

    protected void Initialize(TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
        awareCharacters = new List<Character>();
        InitializeCollisionTrigger();
    }

    /// <summary>
    /// Called when a character starts to do an action towards this object.
    /// </summary>
    /// <param name="action">The current action</param>
    public virtual void OnDoActionToObject(GoapAction action) {
        //owner.SetPOIState(POI_STATE.INACTIVE);
        AddActionToHistory(action);
    }
    /// <summary>
    /// Called when a character finished doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnDoneActionToObject(GoapAction action) {
       
    }
    /// <summary>
    /// Called when a character cancelled doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnCancelActionTowardsObject(GoapAction action) {

    }

    public virtual void SetGridTileLocation(LocationGridTile tile) {
        LocationGridTile previousTile = this.tile;
        this.tile = tile;
        if (tile == null) {
            DisableCollisionTrigger();
            if (previousTile != null) {
                PlaceGhostCollisionTriggerAt(previousTile);
            }
        } else {
            PlaceCollisionTriggerAt(tile);
        }
    }
    public virtual LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    public virtual List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
                    if (goapAction.CanSatisfyRequirements()) {
                        usableActions.Add(goapAction);
                    }
                }
            }
            return usableActions;
        }

        return null;
    }
    public virtual void SetPOIState(POI_STATE state) {
        _state = state;
    }
    public virtual bool IsAvailable() {
        return _state != POI_STATE.INACTIVE;
    }
    public virtual bool IsOwnedBy(Character character) {
        return false;
    }
    /// <summary>
    /// Action to do when the player clicks this object on the map.
    /// </summary>
    public virtual void OnClickAction() { Messenger.Broadcast(Signals.HIDE_MENUS); }

    #region Traits
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (trait.IsUnique() && GetTrait(trait.name) != null) {
            trait.SetCharacterResponsibleForTrait(characterResponsible);
            return false;
        }
        _traits.Add(trait);
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        //ApplyTraitEffects(trait);
        //ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait));
        }
        if (triggerOnAdd) {
            trait.OnAddTrait(this);
        }
        return true;
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true) {
        if (_traits.Remove(trait)) {
            //UnapplyTraitEffects(trait);
            //UnapplyPOITraitInteractions(trait);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this);
            }
            return true;
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true) {
        Trait trait = GetTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                removedTraits.Add(_traits[i]);
                _traits.RemoveAt(i);
                i--;
            }
        }
        return removedTraits;
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName) {
                return _traits[i];
            }
        }
        return null;
    }
    #endregion

    #region Collision
    public void InitializeCollisionTrigger() {
        GameObject collisionGO = GameObject.Instantiate(InteriorMapManager.Instance.poiCollisionTriggerPrefab, InteriorMapManager.Instance.transform);
        SetCollisionTrigger(collisionGO.GetComponent<POICollisionTrigger>());
        collisionGO.SetActive(false);
        _collisionTrigger.Initialize(this);
        RectTransform rt = collisionGO.transform as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
    }
    public void PlaceCollisionTriggerAt(LocationGridTile tile) {
        _collisionTrigger.transform.SetParent(tile.parentAreaMap.objectsParent);
        (_collisionTrigger.transform as RectTransform).anchoredPosition = tile.centeredLocalLocation;
        _collisionTrigger.gameObject.SetActive(true);
        _collisionTrigger.SetLocation(tile);
    }
    public void DisableCollisionTrigger() {
        _collisionTrigger.gameObject.SetActive(false);
    }
    public void SetCollisionTrigger(POICollisionTrigger trigger) {
        _collisionTrigger = trigger;
    }
    public void PlaceGhostCollisionTriggerAt(LocationGridTile tile) {
        GameObject ghostGO = GameObject.Instantiate(InteriorMapManager.Instance.ghostCollisionTriggerPrefab, tile.parentAreaMap.objectsParent);
        RectTransform rt = ghostGO.transform as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        (ghostGO.transform as RectTransform).anchoredPosition = tile.centeredLocalLocation;
        GhostCollisionTrigger gct = ghostGO.GetComponent<GhostCollisionTrigger>();
        gct.Initialize(this);
        gct.SetLocation(tile);
    }
    #endregion

    #region Awareness
    public void AddAwareCharacter(Character character) {
        if (!awareCharacters.Contains(character)) {
            awareCharacters.Add(character);
        }
    }
    public void RemoveAwareCharacter(Character character) {
        //anytime a character aware of this object is removed
        //check if any ghost colliders need to be destroyed
        //to do that:
        if (awareCharacters.Remove(character)) {
            List<LocationGridTile> knownLocations = new List<LocationGridTile>();
            //loop through all the characters that are currently aware of this object
            for (int i = 0; i < awareCharacters.Count; i++) {
                Character currCharacter = awareCharacters[i];
                IAwareness awareness = currCharacter.GetAwareness(this);
                if (!knownLocations.Contains(awareness.knownGridLocation)) {
                    //for each character, store in a list all their known locations of this object
                    knownLocations.Add(awareness.knownGridLocation);
                }
            }
            //then broadcast that list to all ghost colliders of this object
            Messenger.Broadcast(Signals.CHECK_GHOST_COLLIDER_VALIDITY, this as IPointOfInterest, knownLocations);
        }
        //each ghost collider will then check if it's current location is part of the broadcasted list
        //if not, it is safe to destroy that ghost object
        //else keep it
    }
    #endregion

    #region For Testing
    protected void AddActionToHistory(GoapAction action) {
        string summary = GameManager.Instance.ConvertDayToLogString(action.executionDate) + action.actor.name + " performed " + action.goapName;
        actionHistory.Add(summary);
        if (actionHistory.Count > 50) {
            actionHistory.RemoveAt(0);
        }
    }
    public void LogActionHistory() {
        string summary = this.ToString() + "'s action history:";
        for (int i = 0; i < actionHistory.Count; i++) {
            summary += "\n" + (i + 1).ToString() + " - " + actionHistory[i];
        }
        Debug.Log(summary);
    }
    #endregion
}
