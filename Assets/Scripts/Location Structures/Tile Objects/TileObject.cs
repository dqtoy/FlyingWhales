using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Traits;

public class TileObject : IPointOfInterest {

    public string name { get { return ToString(); } }
    public int id { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Faction factionOwner { get { return null; } }
    public List<INTERACTION_TYPE> advertisedActions { get; protected set; }
    public Area specificLocation { get { return gridTileLocation.structure.location; } }
    protected List<Trait> _traits;
    public List<Character> awareCharacters { get; private set; } //characters that are aware of this object (Used for checking if a ghost trigger should be destroyed)
    public List<string> actionHistory { get; private set; } //list of actions that was done to this object
    public LocationStructure structureLocation { get; protected set; }
    public bool isDisabledByPlayer { get; protected set; }
    public bool isSummonedByPlayer { get; protected set; }
    public List<JobQueueItem> allJobsTargettingThis { get; protected set; }
    //public List<GoapAction> targettedByAction { get; protected set; }
    public List<Character> owners { get; private set; }
    public virtual Character[] users {
        get {
            if (slots == null) {
                return null;
            }
            return slots.Where(x => x != null && x.user != null).Select(x => x.user).ToArray();
        }
    }//array of characters, currently using the tile object
    public Character removedBy { get; private set; }

    //hp
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }

    ///this is null by default. This is responsible for updating the pathfinding graph when a tileobject that should be unapassable is placed <see cref="LocationGridTileGUS.Initialize(Vector3[])"/>, this should also destroyed when the object is removed. <see cref="LocationGridTileGUS.Destroy"/>
    public LocationGridTileGUS graphUpdateScene { get; protected set; } 

    //tile slots
    public TileObjectSlotItem[] slots { get; protected set; } //for users
    private GameObject slotsParent;
    protected bool hasCreatedSlots;

    protected LocationGridTile tile;
    private POI_STATE _state;
    protected POICollisionTrigger _collisionTrigger;
    public LocationGridTile previousTile { get; protected set; }

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
    public Vector3 worldPosition {
        get { return gridTileLocation.centeredWorldLocation; }
    }
    public bool isDead {
        get { return gridTileLocation == null; } //Consider the object as dead if it no longer has a tile location (has been removed)
    }
    public ProjectileReceiver projectileReciever {
        get { return collisionTrigger.projectileReciever; }
    }
    #endregion

    protected void Initialize(TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
        awareCharacters = new List<Character>();
        allJobsTargettingThis = new List<JobQueueItem>();
        //targettedByAction = new List<GoapAction>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        maxHP = TileObjectDB.GetTileObjectData(tileObjectType).maxHP;
        currentHP = maxHP;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        InitializeCollisionTrigger();
        AddCommonAdvertisments();
        InteriorMapManager.Instance.AddTileObject(this);
    }
    protected void Initialize(SaveDataTileObject data) {
        id = Utilities.SetID(this, data.id);
        tileObjectType = data.tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
        awareCharacters = new List<Character>();
        allJobsTargettingThis = new List<JobQueueItem>();
        owners = new List<Character>();
        //targettedByAction = new List<GoapAction>();
        hasCreatedSlots = false;
        CreateTraitContainer();
        InitializeCollisionTrigger();
        AddCommonAdvertisments();
        InteriorMapManager.Instance.AddTileObject(this);
    }

    private void AddCommonAdvertisments() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.POISON);
    }

    #region Virtuals
    /// <summary>
    /// Called when a character starts to do an action towards this object.
    /// </summary>
    /// <param name="action">The current action</param>
    public virtual void OnDoActionToObject(ActualGoapNode action) {
        //owner.SetPOIState(POI_STATE.INACTIVE);
    }
    /// <summary>
    /// Called when a character finished doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnDoneActionToObject(ActualGoapNode action) {

    }
    /// <summary>
    /// Called when a character cancelled doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnCancelActionTowardsObject(ActualGoapNode action) {

    }
    public virtual void SetGridTileLocation(LocationGridTile tile) {
        previousTile = this.tile;
        this.tile = tile;
        if (_collisionTrigger == null) {
            InitializeCollisionTrigger();
        }
        if (tile == null) {
            DisableCollisionTrigger();
            OnRemoveTileObject(null, previousTile);
            SetPOIState(POI_STATE.INACTIVE);
        } else {
            PlaceCollisionTriggerAt(tile);
            OnPlaceObjectAtTile(tile);
            SetPOIState(POI_STATE.ACTIVE);
        }
    }
    public virtual void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.tile;
        this.tile = null;
        DisableCollisionTrigger();
        OnRemoveTileObject(removedBy, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
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
    public INTERACTION_TYPE AdvertiseActionsToActor(Character actor, GoapEffect precondition, Dictionary<INTERACTION_TYPE, object[]> otherData) {
        INTERACTION_TYPE chosenAction = INTERACTION_TYPE.NONE;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isCharacterAvailable = IsAvailable();
            //List<GoapAction> usableActions = new List<GoapAction>();
            INTERACTION_TYPE lowestCostAction = INTERACTION_TYPE.NONE;
            int currentLowestCost = 0;
            for (int i = 0; i < advertisedActions.Count; i++) {
                INTERACTION_TYPE currType = advertisedActions[i];
                GoapAction action = InteractionManager.Instance.goapActionData[currType];
                if (!isCharacterAvailable && !action.canBeAdvertisedEvenIfActorIsUnavailable) {
                    //if this character is not available, check if the current action type can be advertised even when the character is inactive.
                    continue; //skip
                }
                if (RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    object[] otherActionData = null;
                    if (otherData.ContainsKey(currType)) {
                        otherActionData = otherData[currType];
                    }
                    if (action.CanSatisfyRequirements(actor, this, data)
                        && action.WillEffectsSatisfyPrecondition(precondition, this, data)) { //&& InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)
                        int actionCost = action.GetCost(actor, this, data);
                        if (lowestCostAction == INTERACTION_TYPE.NONE || actionCost < currentLowestCost) {
                            lowestCostAction = action.goapType;
                            currentLowestCost = actionCost;
                        }
                        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        //if (goapAction != null) {
                        //    if (data != null) {
                        //        goapAction.InitializeOtherData(data);
                        //    }
                        //    usableActions.Add(goapAction);
                        //} else {
                        //    throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        //}
                    }
                }
            }
            if (lowestCostAction != INTERACTION_TYPE.NONE) {
                chosenAction = lowestCostAction;
            }
            //return usableActions;
        }
        return chosenAction;
    }
    public virtual void SetPOIState(POI_STATE state) {
        _state = state;
    }
    public virtual bool IsOwnedBy(Character character) {
        return owners.Contains(character);
    }
    /// <summary>
    /// Action to do when the player clicks this object on the map.
    /// </summary>
    public virtual void OnClickAction() { Messenger.Broadcast(Signals.HIDE_MENUS); }
    /// <summary>
    /// Triggered when the grid tile location of this object is set to null.
    /// </summary>
    protected virtual void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        Debug.Log(GameManager.Instance.TodayLogString() + "Tile Object " + this.name + " has been removed");
        this.removedBy = removedBy;
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this, removedBy, removedFrom);
        if (hasCreatedSlots) {
            DestroyTileSlots();
        }
        traitContainer.RemoveAllTraits(this);
    }
    public virtual bool CanBeReplaced() {
        return false;
    }
    public virtual void OnTileObjectGainedTrait(Trait trait) { }
    public virtual void SetStructureLocation(LocationStructure structure) {
        structureLocation = structure;
    }
    public virtual bool IsValidCombatTarget() {
        return gridTileLocation != null;
    }
    #endregion

    #region IPointOfInterest
    public bool IsAvailable() {
        return _state != POI_STATE.INACTIVE && !isDisabledByPlayer;
    }
    public void SetIsDisabledByPlayer(bool state) {
        if(isDisabledByPlayer != state) {
            isDisabledByPlayer = state;
            if (isDisabledByPlayer) {
                Character character = null;
                Messenger.Broadcast(Signals.TILE_OBJECT_DISABLED, this, character);
            }
        }
    }
    public void SetIsSummonedByPlayer(bool state) {
        if(isSummonedByPlayer != state) {
            isSummonedByPlayer = state;
            if (isSummonedByPlayer) {
                if(advertisedActions == null) {
                    advertisedActions = new List<INTERACTION_TYPE>();
                }
                if (!advertisedActions.Contains(INTERACTION_TYPE.INSPECT)) {
                    advertisedActions.Add(INTERACTION_TYPE.INSPECT);
                }
            } else {
                if (advertisedActions != null) {
                    advertisedActions.Remove(INTERACTION_TYPE.INSPECT);
                }
            }
        }
    }
    public void AddAdvertisedAction(INTERACTION_TYPE type) {
        if (advertisedActions == null) {
            advertisedActions = new List<INTERACTION_TYPE>();
        }
        advertisedActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Add(type);
    }
    public void AddJobTargettingThis(JobQueueItem job) {
        allJobsTargettingThis.Add(job);
    }
    public bool RemoveJobTargettingThis(JobQueueItem job) {
        return allJobsTargettingThis.Remove(job);
    }
    public bool HasJobTargettingThis(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            for (int j = 0; j < jobTypes.Length; j++) {
                if (job.jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType) {
                    return job;
                }
            }
        }
        return null;
    }
    //public void AddTargettedByAction(GoapAction action) {
    //    targettedByAction.Add(action);
    //}
    //public void RemoveTargettedByAction(GoapAction action) {
    //    targettedByAction.Remove(action);
    //}
    public void AdjustHP(int amount, bool triggerDeath = false, object source = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        this.currentHP += amount;
        this.currentHP = Mathf.Clamp(this.currentHP, 0, maxHP);
        if (currentHP == 0) {
            //object has been destroyed
            Character removedBy = null;
            if (source is Character) {
                removedBy = source as Character;
            }
            gridTileLocation.structure.RemovePOI(this, removedBy);
        }
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        GameManager.Instance.CreateHitEffectAt(this);
        if (this.currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }
        this.AdjustHP(-characterThatAttacked.attackPower, source: characterThatAttacked);
        attackSummary += "\nDealt damage " + characterThatAttacked.attackPower.ToString();
        if (this.currentHP <= 0) {
            attackSummary += "\n" + this.name + "'s hp has reached 0.";
        }
        //Messenger.Broadcast(Signals.CHARACTER_WAS_HIT, this, characterThatAttacked);
    }
    #endregion

    #region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor { get { return TraitManager.tileObjectTraitProcessor; } }
    private void CreateTraitContainer() {
        traitContainer = new TraitContainer();
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
    public virtual void DisableCollisionTrigger() {
        _collisionTrigger.gameObject.SetActive(false);
    }
    public virtual void EnableCollisionTrigger() {
        _collisionTrigger.gameObject.SetActive(true);
    }
    public void SetCollisionTrigger(POICollisionTrigger trigger) {
        _collisionTrigger = trigger;
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
                //IAwareness awareness = currCharacter.HasAwareness(this);
                if (gridTileLocation != null && !knownLocations.Contains(gridTileLocation)) {
                    //for each character, store in a list all their known locations of this object
                    knownLocations.Add(gridTileLocation);
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

    #region GOAP
    private void ConstructInitialGoapAdvertisements() {
        advertisedActions.Add(INTERACTION_TYPE.INSPECT);
    }
    /// <summary>
    /// Does this tile object advertise a given action type.
    /// </summary>
    /// <param name="type">The action type that need to be advertised.</param>
    /// <returns>If this tile object advertises the given action.</returns>
    public bool Advertises(INTERACTION_TYPE type) {
        return advertisedActions.Contains(type);
    }
    /// <summary>
    /// Does this tile object advertise all of the given actions.
    /// </summary>
    /// <param name="types">The action types that need to be advertised.</param>
    /// <returns>If this tile object meets all the requirements.</returns>
    public bool AdvertisesAll(params INTERACTION_TYPE[] types) {
        for (int i = 0; i < types.Length; i++) {
            if (!(Advertises(types[i]))) {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Does this tile object advertise a number of types from the given list.
    /// </summary>
    /// <param name="count">The number of valid types to consider.</param>
    /// <param name="types">The list of types to check.</param>
    /// <returns>If this tile object can meet the needed requirements.</returns>
    public bool AdvertisesAny(int count = 1, params INTERACTION_TYPE[] types) {
        int validCount = 0;
        for (int i = 0; i < types.Length; i++) {
            if ((Advertises(types[i]))) {
                validCount++;
                if (validCount >= count) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapAction Advertise(INTERACTION_TYPE type, Character actor) {
        GoapAction goapAction = InteractionManager.Instance.goapActionData[type];
        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(type, actor, this, null)) {
            return goapAction;
        }
        return null;
    }
    #endregion

    #region Tile Object Slots
    protected virtual void OnPlaceObjectAtTile(LocationGridTile tile) {
        removedBy = null;
        if (hasCreatedSlots) {
            RepositionTileSlots(tile);
        } else {
            CreateTileObjectSlots();
        }
        UpdateOwners();
        Messenger.Broadcast(Signals.TILE_OBJECT_PLACED, this, tile);
    }
    private void CreateTileObjectSlots() {
        UnityEngine.Tilemaps.TileBase usedAsset = tile.parentAreaMap.objectsTilemap.GetTile(tile.localPlace);
        if (tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && usedAsset != null && InteriorMapManager.Instance.HasSettingForTileObjectAsset(usedAsset)) {
            List<TileObjectSlotSetting> slotSettings = InteriorMapManager.Instance.GetTileObjectSlotSettings(usedAsset);
            slotsParent = GameObject.Instantiate(InteriorMapManager.Instance.tileObjectSlotsParentPrefab, tile.parentAreaMap.objectsTilemap.transform);
            slotsParent.transform.localPosition = tile.centeredLocalLocation;
            slotsParent.transform.localRotation = tile.parentAreaMap.objectsTilemap.GetTransformMatrix(tile.localPlace).rotation;
            slotsParent.name = this.ToString() + " Slots";
            slots = new TileObjectSlotItem[slotSettings.Count];
            for (int i = 0; i < slotSettings.Count; i++) {
                TileObjectSlotSetting currSetting = slotSettings[i];
                GameObject currSlot = GameObject.Instantiate(InteriorMapManager.Instance.tileObjectSlotPrefab, Vector3.zero, Quaternion.identity, slotsParent.transform);
                TileObjectSlotItem currSlotItem = currSlot.GetComponent<TileObjectSlotItem>();
                currSlotItem.ApplySettings(this, currSetting);
                slots[i] = currSlotItem;
            }
        }
        hasCreatedSlots = true;
    }
    private void RepositionTileSlots(LocationGridTile tile) {
        if (slotsParent != null) {
            slotsParent.transform.localPosition = tile.centeredLocalLocation;
        }
    }
    protected void DestroyTileSlots() {
        if (slots == null) {
            return;
        }
        for (int i = 0; i < slots.Length; i++) {
            GameObject.Destroy(slots[i].gameObject);
        }
        slots = null;
        hasCreatedSlots = false;
    }
    private TileObjectSlotItem GetNearestUnoccupiedSlot(Character character) {
        float nearest = 9999f;
        TileObjectSlotItem nearestSlot = null;
        for (int i = 0; i < slots.Length; i++) {
            TileObjectSlotItem slot = slots[i];
            if (slot.user == null) {
                float distance = Vector2.Distance(character.marker.transform.position, slot.transform.position);
                if (distance < nearest) {
                    nearest = distance;
                    nearestSlot = slot;
                }
            }
        }
        return nearestSlot;
    }
    private bool HasUnoccupiedSlot() {
        for (int i = 0; i < slots.Length; i++) {
            TileObjectSlotItem slot = slots[i];
            if (slot.user == null) {
                return true;
            }
        }
        return false;
    }
    private TileObjectSlotItem GetSlotUsedBy(Character character) {
        for (int i = 0; i < slots.Length; i++) {
            TileObjectSlotItem slot = slots[i];
            if (slot.user == character) {
                return slot;
            }
        }
        return null;
    }
    public void SetSlotColor(Color color) {
        if (slots != null) {
            for (int i = 0; i < slots.Length; i++) {
                TileObjectSlotItem slot = slots[i];
                slot.SetSlotColor(color);
            }
        }
    }
    #endregion

    #region Users
    public virtual void AddUser(Character newUser) {
        if (users.Contains(newUser)) {
            return;
        }
        TileObjectSlotItem availableSlot = GetNearestUnoccupiedSlot(newUser);
        if (availableSlot != null) {
            newUser.SetTileObjectLocation(this);
            availableSlot.Use(newUser);
            if (!HasUnoccupiedSlot()) {
                SetPOIState(POI_STATE.INACTIVE);
            }
        }
    }
    protected virtual void RemoveUser(Character user) {
        TileObjectSlotItem slot = GetSlotUsedBy(user);
        if (slot != null) {
            user.SetTileObjectLocation(null);
            slot.StopUsing();
            SetPOIState(POI_STATE.ACTIVE);
        }
    }
    #endregion

    #region Utilities
    public void DoCleanup() {
        traitContainer.RemoveAllTraits(this);
    }
    public void UpdateOwners() {
        if (tile.structure is Dwelling) {
            owners.Clear();
            owners.AddRange((tile.structure as Dwelling).residents);
        }
    }
    #endregion

    #region Inspect
    public virtual void OnInspect(Character inspector) { //, out Log log
        //if (LocalizationManager.Instance.HasLocalizedValue("TileObject", this.GetType().ToString(), "on_inspect")) {
        //    log = new Log(GameManager.Instance.Today(), "TileObject", this.GetType().ToString(), "on_inspect");
        //} else {
        //    log = null;
        //}
        
    }
    #endregion

    #region Graph Updates
    protected void CreateNewGUS(Vector2 offset, Vector2 size) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocationGridTileGUS", Vector3.zero, Quaternion.identity, gridTileLocation.parentAreaMap.graphUpdateScenesParent);
        LocationGridTileGUS gus = go.GetComponent<LocationGridTileGUS>();
        gus.Initialize(offset, size, this);
        this.graphUpdateScene = gus;
    }
    protected void DestroyExistingGUS() {
        if (this.graphUpdateScene != null) {
            this.graphUpdateScene.Destroy();
        }
    }
    #endregion
}

[System.Serializable]
public struct TileObjectSlotSetting {
    public string slotName;
    public Vector3 characterPosition;
    public Vector3 usedPosition;
    public Vector3 unusedPosition;
    public Vector3 assetRotation;
    public Sprite slotAsset;
}

[System.Serializable]
public struct TileObjectSerializableData {
    public int id;
    public TILE_OBJECT_TYPE type;
}

[System.Serializable]
public class SaveDataTileObject {
    public int id;
    public TILE_OBJECT_TYPE tileObjectType;
    public List<SaveDataTrait> traits;
    public List<int> awareCharactersIDs;
    //public LocationStructure structureLocation { get; protected set; }
    public bool isDisabledByPlayer;
    public bool isSummonedByPlayer;
    //public List<JobQueueItem> allJobsTargettingThis { get; protected set; }
    //public List<Character> owners;

    //public Vector3Save tileID;
    public POI_STATE state;
    public Vector3Save previousTileID;
    public int previousTileAreaID;
    public bool hasCurrentTile;

    public int structureLocationAreaID;
    public int structureLocationID;
    public STRUCTURE_TYPE structureLocationType;

    protected TileObject loadedTileObject;

    public virtual void Save(TileObject tileObject) {
        id = tileObject.id;
        tileObjectType = tileObject.tileObjectType;
        isDisabledByPlayer = tileObject.isDisabledByPlayer;
        isSummonedByPlayer = tileObject.isSummonedByPlayer;
        state = tileObject.state;

        hasCurrentTile = tileObject.gridTileLocation != null;

        if(tileObject.structureLocation != null) {
            structureLocationID = tileObject.structureLocation.id;
            structureLocationAreaID = tileObject.structureLocation.location.id;
            structureLocationType = tileObject.structureLocation.structureType;
        } else {
            structureLocationID = -1;
            structureLocationAreaID = -1;
        }

        if (tileObject.previousTile != null) {
            previousTileID = new Vector3Save(tileObject.previousTile.localPlace);
            previousTileAreaID = tileObject.previousTile.structure.location.id;
        } else {
            previousTileID = new Vector3Save(0, 0, -1);
            previousTileAreaID = -1;
        }

        traits = new List<SaveDataTrait>();
        for (int i = 0; i < tileObject.traitContainer.allTraits.Count; i++) {
            SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(tileObject.traitContainer.allTraits[i]);
            if (saveDataTrait != null) {
                saveDataTrait.Save(tileObject.traitContainer.allTraits[i]);
                traits.Add(saveDataTrait);
            }
        }

        awareCharactersIDs = new List<int>();
        for (int i = 0; i < tileObject.awareCharacters.Count; i++) {
            awareCharactersIDs.Add(tileObject.awareCharacters[i].id);
        }
    }

    public virtual TileObject Load() {
        string tileObjectName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObjectType.ToString());
        TileObject tileObject = System.Activator.CreateInstance(System.Type.GetType(tileObjectName), this) as TileObject;

        if(structureLocationID != -1 && structureLocationAreaID != -1) {
            Area area = LandmarkManager.Instance.GetAreaByID(structureLocationAreaID);
            tileObject.SetStructureLocation(area.GetStructureByID(structureLocationType, structureLocationID));
        }
        for (int i = 0; i < awareCharactersIDs.Count; i++) {
            tileObject.AddAwareCharacter(CharacterManager.Instance.GetCharacterByID(awareCharactersIDs[i]));
        }

        tileObject.SetIsDisabledByPlayer(isDisabledByPlayer);
        tileObject.SetIsSummonedByPlayer(isSummonedByPlayer);
        tileObject.SetPOIState(state);

        loadedTileObject = tileObject;
        return loadedTileObject;
    }

    //This is the last to be loaded in SaveDataTileObject, so release loadedTileObject reference
    public virtual void LoadAfterLoadingAreaMap() {
        loadedTileObject = null;
    }

    public void LoadPreviousTileAndCurrentTile() {
        if (previousTileAreaID != -1 && previousTileID.z != -1) {
            Area area = LandmarkManager.Instance.GetAreaByID(previousTileAreaID);
            LocationGridTile tile = area.areaMap.map[(int)previousTileID.x, (int)previousTileID.y];
            tile.structure.AddPOI(loadedTileObject, tile);
            if (!hasCurrentTile) {
                tile.structure.RemovePOI(loadedTileObject);
            }
        }
    }

    public void LoadTraits() {
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            loadedTileObject.traitContainer.AddTrait(loadedTileObject, trait, responsibleCharacter);
        }
    }
}