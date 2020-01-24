using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actionables;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;
using UnityEngine.Experimental.U2D;

public abstract class TileObject : MapObject<TileObject>, IPointOfInterest, IPlayerActionTarget {
    public string name { get; protected set; }
    public int id { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Faction factionOwner { get { return null; } }
    public List<INTERACTION_TYPE> advertisedActions { get; protected set; }
    public Region currentRegion { get { return gridTileLocation.structure.location.coreTile.region; } }
    public List<string> actionHistory { get; private set; } //list of actions that was done to this object
    public LocationStructure structureLocation { get { return gridTileLocation.structure; } }
    public bool isDisabledByPlayer { get; protected set; }
    public bool isSummonedByPlayer { get; protected set; }
    public List<JobQueueItem> allJobsTargetingThis { get; protected set; }
    public List<Character> owners { get; private set; }
    public virtual Character[] users {
        get {
            return slots?.Where(x => x != null && x.user != null).Select(x => x.user).ToArray() ?? null;
        }
    }//array of characters, currently using the tile object
    public Character removedBy { get; private set; }
    public BaseMapObjectVisual mapObjectVisual => mapVisual;

    //hp
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }

    ///this is null by default. This is responsible for updating the pathfinding graph when a tileobject that should be unapassable is placed <see cref="LocationGridTileGUS.Initialize(Vector3[])"/>, this should also destroyed when the object is removed. <see cref="LocationGridTileGUS.Destroy"/>
    public LocationGridTileGUS graphUpdateScene { get; protected set; } 

    //tile slots
    public TileObjectSlotItem[] slots { get; protected set; } //for users
    private GameObject slotsParent;
    protected bool hasCreatedSlots;

    public virtual LocationGridTile gridTileLocation { get; protected set; }
    public POI_STATE state { get; private set; }
    public LocationGridTile previousTile { get; protected set; }
    public Character isBeingCarriedBy { get; protected set; }
    public Dictionary<RESOURCE, int> storedResources { get; protected set; }
    protected Dictionary<RESOURCE, int> maxResourceValues { get; set; }

    private bool hasSubscribedToListeners;
    
    #region getters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.TILE_OBJECT; }
    }
    public Vector3 worldPosition {
        get { return gridTileLocation.centeredWorldLocation; }
    }
    public bool isDead {
        get { return gridTileLocation == null; } //Consider the object as dead if it no longer has a tile location (has been removed)
    }
    public ProjectileReceiver projectileReceiver {
        get { return mapVisual.collisionTrigger.projectileReceiver; }
    }
    public Transform worldObject { get { return mapVisual.transform; } }
    public string nameWithID => ToString();
    #endregion

    protected void Initialize(TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.tileObjectType = tileObjectType;
        name = Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectType.ToString());
        actionHistory = new List<string>();
        allJobsTargetingThis = new List<JobQueueItem>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        maxHP = TileObjectDB.GetTileObjectData(tileObjectType).maxHP;
        currentHP = maxHP;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        ConstructResources();
        AddCommonAdvertisements();
        ConstructDefaultActions();
        InnerMapManager.Instance.AddTileObject(this);
        SubscribeListeners();
    }
    protected void Initialize(SaveDataTileObject data) {
        id = Utilities.SetID(this, data.id);
        tileObjectType = data.tileObjectType;
        actionHistory = new List<string>();
        allJobsTargetingThis = new List<JobQueueItem>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        CreateTraitContainer();
        AddCommonAdvertisements();
        ConstructResources();
        ConstructDefaultActions();
        InnerMapManager.Instance.AddTileObject(this);
        SubscribeListeners();
    }

    private void AddCommonAdvertisements() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.POISON);
        AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
    }
    protected void RemoveCommonAdvertisements() {
        RemoveAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        RemoveAdvertisedAction(INTERACTION_TYPE.POISON);
        RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
    }

    #region Listeners
    protected void SubscribeListeners() {
        // if (hasSubscribedToListeners == false) {
        //     hasSubscribedToListeners = true;
        //     Messenger.AddListener(Signals.TICK_ENDED, () => traitContainer.ProcessOnTickEnded(this));    
        // }
    }
    protected void UnsubscribeListeners() {
        // if (hasSubscribedToListeners) {
        //     hasSubscribedToListeners = false;
        //     Messenger.RemoveListener(Signals.TICK_ENDED, () => traitContainer.ProcessOnTickEnded(this));    
        // }
    }
    #endregion
    
    #region Virtuals
    /// <summary>
    /// Called when a character starts to do an action towards this object.
    /// </summary>
    /// <param name="action">The current action</param>
    public virtual void OnDoActionToObject(ActualGoapNode action) { }
    /// <summary>
    /// Called when a character finished doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnDoneActionToObject(ActualGoapNode action) { }
    /// <summary>
    /// Called when a character cancelled doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnCancelActionTowardsObject(ActualGoapNode action) { }

    public virtual void OnDestroyPOI() {
        //DisableGameObject();
        DestroyGameObject();
        OnRemoveTileObject(null, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
        TileObjectData objData;
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                UnoccupyTiles(objData.occupiedSize, previousTile);
            }
        }
        Messenger.Broadcast(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        UnsubscribeListeners();
    }
    public virtual void OnPlacePOI() {
        SetPOIState(POI_STATE.ACTIVE);
        if (mapVisual == null) {
            InitializeMapObject(this);
            gridTileLocation.parentMap.location.AddAwareness(this);
        }
        PlaceMapObjectAt(gridTileLocation);
        OnPlaceTileObjectAtTile(gridTileLocation);
        TileObjectData objData;
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                OccupyTiles(objData.occupiedSize, gridTileLocation);
            }
        }
        SubscribeListeners();
    }
    public virtual void RemoveTileObject(Character removedBy) {
        SetGridTileLocation(null);
        //DisableGameObject();
        //DestroyGameObject();
        //OnRemoveTileObject(removedBy, previousTile);
        //SetPOIState(POI_STATE.INACTIVE);
        OnDestroyPOI();
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
    //Returns the chosen action for the plan
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost, ref string log) {
        GoapAction chosenAction = null;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isCharacterAvailable = IsAvailable();
            //List<GoapAction> usableActions = new List<GoapAction>();
            GoapAction lowestCostAction = null;
            int currentLowestCost = 0;
            log += "\n--Choices for " + precondition.ToString();
            log += "\n--";
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
                    //object[] otherActionData = null;
                    //if (otherData.ContainsKey(currType)) {
                    //    otherActionData = otherData[currType];
                    //}
                    if (action.CanSatisfyRequirements(actor, this, data)
                        && action.WillEffectsSatisfyPrecondition(precondition, actor, this, data)) { //&& InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)
                        int actionCost = action.GetCost(actor, this, data);
                        log += "(" + actionCost + ")" + action.goapName + "-" + nameWithID + ", ";
                        if (lowestCostAction == null || actionCost < currentLowestCost) {
                            lowestCostAction = action;
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
            cost = currentLowestCost;
            chosenAction = lowestCostAction;
            //return usableActions;
        }
        return chosenAction;
    }
    public bool CanAdvertiseActionToActor(Character actor, GoapAction action, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost) {
        if ((IsAvailable() || action.canBeAdvertisedEvenIfActorIsUnavailable)
            && advertisedActions != null && advertisedActions.Contains(action.goapType)
            && actor.trapStructure.SatisfiesForcedStructure(this)
            && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType)) {
            object[] data = null;
            if (otherData != null) {
                if (otherData.ContainsKey(action.goapType)) {
                    data = otherData[action.goapType];
                } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                    data = otherData[INTERACTION_TYPE.NONE];
                }
            }
            if (action.CanSatisfyRequirements(actor, this, data)) {
                cost = action.GetCost(actor, this, data);
                return true;
            }
        }
        return false;
    }
    public virtual void SetPOIState(POI_STATE state) {
        this.state = state;
    }
    /// <summary>
    /// Triggered when the grid tile location of this object is set to null.
    /// </summary>
    protected virtual void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        // Debug.Log(GameManager.Instance.TodayLogString() + "Tile Object " + this.name + " has been removed");
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
    public virtual void OnTileObjectLostTrait(Trait trait) { }
    public virtual bool IsValidCombatTarget() {
        return gridTileLocation != null;
    }
    public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character) {
        if(mapVisual == null) {
            return false;
        }
        if(gridTileLocation != null && currentRegion == character.currentRegion) {
            return true;
        }else if (isBeingCarriedBy != null && isBeingCarriedBy.currentRegion == character.currentRegion) {
            return true;
        }
        return false;
    }
    #endregion

    #region IPointOfInterest
    public bool IsAvailable() {
        return state != POI_STATE.INACTIVE && !isDisabledByPlayer;
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
        mapVisual?.UpdateCollidersState(this);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Remove(type);
        mapVisual?.UpdateCollidersState(this);
    }
    public void AddJobTargetingThis(JobQueueItem job) {
        allJobsTargetingThis.Add(job);
    }
    public bool RemoveJobTargetingThis(JobQueueItem job) {
        return allJobsTargetingThis.Remove(job);
    }
    public bool HasJobTargetingThis(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            for (int j = 0; j < jobTypes.Length; j++) {
                if (job.jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlanJob GetJobTargetingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType) {
                    return job;
                }
            }
        }
        return null;
    }
    public virtual void AdjustHP(int amount, bool triggerDeath = false, object source = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        if (amount < 0 && source != null) {
            GameManager.Instance.CreateHitEffectAt(this);
        }
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (currentHP == 0) {
            //object has been destroyed
            Character removed = null;
            if (source is Character) {
                removed = source as Character;
            }
            gridTileLocation.structure.RemovePOI(this, removed);
        }
        if (amount < 0) {
            Messenger.Broadcast(Signals.OBJECT_DAMAGED, this as IPointOfInterest);    
        } else if (currentHP == maxHP) {
            Messenger.Broadcast(Signals.OBJECT_REPAIRED, this as IPointOfInterest);
        }
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        GameManager.Instance.CreateHitEffectAt(this);
        if (currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }
        AdjustHP(-characterThatAttacked.attackPower, source: characterThatAttacked);
        attackSummary += "\nDealt damage " + characterThatAttacked.attackPower.ToString();
        if (currentHP <= 0) {
            attackSummary += "\n" + name + "'s hp has reached 0.";
        }
        //Messenger.Broadcast(Signals.CHARACTER_WAS_HIT, this, characterThatAttacked);
    }
    public void SetGridTileLocation(LocationGridTile tile) {
        previousTile = gridTileLocation;
        gridTileLocation = tile;
    }
    public void OnSeizePOI() {
        if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == this) {
            UIManager.Instance.tileObjectInfoUI.CloseMenu();
        }
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
        Messenger.Broadcast(Signals.ON_SEIZE_TILE_OBJECT, this);

        gridTileLocation.structure.RemovePOI(this);
    }
    public void OnUnseizePOI(LocationGridTile tileLocation) {
        tileLocation.structure.AddPOI(this, tileLocation);
    }
    #endregion

    #region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor { get { return TraitManager.tileObjectTraitProcessor; } }
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    public LocationGridTile GetGridTileLocation(AreaInnerTileMap map) {
        return gridTileLocation;
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
    #endregion

    #region Tile Object Slots
    protected virtual void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        removedBy = null;
        CheckFurnitureSettings();
        if (hasCreatedSlots) {
            RepositionTileSlots(tile);
        } else {
            CreateTileObjectSlots();
        }
        UpdateOwners();
        Messenger.Broadcast(Signals.TILE_OBJECT_PLACED, this, tile);
    }
    private void CreateTileObjectSlots() {
        Sprite usedAsset = mapVisual.usedSprite;
        if (tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && usedAsset != null && InnerMapManager.Instance.HasSettingForTileObjectAsset(usedAsset)) {
            List<TileObjectSlotSetting> slotSettings = InnerMapManager.Instance.GetTileObjectSlotSettings(usedAsset);
            slotsParent = Object.Instantiate(InnerMapManager.Instance.tileObjectSlotsParentPrefab, mapVisual.transform);
            slotsParent.transform.localPosition = Vector3.zero;
            slotsParent.name = ToString() + " Slots";
            slots = new TileObjectSlotItem[slotSettings.Count];
            for (int i = 0; i < slotSettings.Count; i++) {
                TileObjectSlotSetting currSetting = slotSettings[i];
                GameObject currSlot = Object.Instantiate(InnerMapManager.Instance.tileObjectSlotPrefab, Vector3.zero, Quaternion.identity, slotsParent.transform);
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
            Object.Destroy(slots[i].gameObject);
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
        if (slots != null) {
            for (int i = 0; i < slots.Length; i++) {
                TileObjectSlotItem slot = slots[i];
                if (slot.user == character) {
                    return slot;
                }
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
    public void SetSlotAlpha(float alpha) {
        if (slots != null) {
            for (int i = 0; i < slots.Length; i++) {
                TileObjectSlotItem slot = slots[i];
                Color color = slot.spriteRenderer.color;
                color.a = alpha;
                slot.SetSlotColor(color);
            }
        }
    }
    public void RevalidateTileObjectSlots() {
        if (hasCreatedSlots) {
            DestroyTileSlots();
            CreateTileObjectSlots();
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
    public virtual bool RemoveUser(Character user) {
        TileObjectSlotItem slot = GetSlotUsedBy(user);
        if (slot != null) {
            user.SetTileObjectLocation(null);
            slot.StopUsing();
            SetPOIState(POI_STATE.ACTIVE);
            return true;
        }
        return false;
    }
    #endregion

    #region Utilities
    public void DoCleanup() {
        traitContainer.RemoveAllTraits(this);
    }
    public void UpdateOwners() {
        if (gridTileLocation.structure is Dwelling) {
            owners.Clear();
            owners.AddRange((gridTileLocation.structure as Dwelling).residents);
        }
    }
    public void SetIsBeingCarriedBy(Character carrier) {
        isBeingCarriedBy = carrier;
    }
    public virtual bool CanBeDamaged() {
        return mapObjectState != MAP_OBJECT_STATE.UNBUILT;
    }
    private void OccupyTiles(Point size, LocationGridTile tile) {
        List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(size, tile);
        for (int i = 0; i < overlappedTiles.Count; i++) {
            LocationGridTile currTile = overlappedTiles[i];
            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
        }
    }
    private void UnoccupyTiles(Point size, LocationGridTile tile) {
        List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(size, tile);
        for (int i = 0; i < overlappedTiles.Count; i++) {
            LocationGridTile currTile = overlappedTiles[i];
            currTile.SetTileState(LocationGridTile.Tile_State.Empty);
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
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocationGridTileGUS", Vector3.zero, Quaternion.identity, mapVisual.transform);//gridTileLocation.parentAreaMap.graphUpdateScenesParent
        LocationGridTileGUS gus = go.GetComponent<LocationGridTileGUS>();
        gus.Initialize(offset, size, this);
        graphUpdateScene = gus;
    }
    protected void DestroyExistingGUS() {
        if (graphUpdateScene != null) {
            graphUpdateScene.Destroy();
        }
    }
    #endregion

    #region Visuals
    private void CheckFurnitureSettings() {
        if (gridTileLocation.hasFurnitureSpot) {
            FurnitureSetting furnitureSetting;
            if (gridTileLocation.furnitureSpot.TryGetFurnitureSettings(tileObjectType.ConvertTileObjectToFurniture(), out furnitureSetting)) {
                mapVisual.ApplyFurnitureSettings(furnitureSetting);
            }
        }
    }
    #endregion

    #region Map Object
    protected override void CreateAreaMapGameObject() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectAreaMapObject(tileObjectType);
        mapVisual = obj.GetComponent<TileObjectGameObject>();
    }
    private INTERACTION_TYPE[] storedActions;
    protected override void OnMapObjectStateChanged() {
        if (mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            mapVisual.SetVisualAlpha(0f / 255f);
            SetSlotAlpha(0f / 255f);
            //store advertised actions
            storedActions = new INTERACTION_TYPE[advertisedActions.Count];
            for (int i = 0; i < advertisedActions.Count; i++) {
                storedActions[i] = advertisedActions[i];
            }
            advertisedActions.Clear();
            AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            UnsubscribeListeners();
        } else if (mapObjectState == MAP_OBJECT_STATE.BUILDING) {
            mapVisual.SetVisualAlpha(128f / 255f);
            SetSlotAlpha(128f / 255f);
        } else {
            mapVisual.SetVisualAlpha(255f / 255f);
            SetSlotAlpha(255f / 255f);
            RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            for (int i = 0; i < storedActions.Length; i++) {
                AddAdvertisedAction(storedActions[i]);
            }
            storedActions = null;
            SubscribeListeners();
        }
    }
    #endregion

    #region Resources
    public void ConstructResources() {
        storedResources = new Dictionary<RESOURCE, int>() {
            { RESOURCE.FOOD, 0 },
            { RESOURCE.WOOD, 0 },
            { RESOURCE.STONE, 0 },
            { RESOURCE.METAL, 0 },
        };
        ConstructMaxResources();
    }
    protected virtual void ConstructMaxResources() {
        maxResourceValues = new Dictionary<RESOURCE, int>();
        RESOURCE[] resourceTypes = Utilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            maxResourceValues.Add(resourceType, 1000);
        }
    }
    public void SetResource(RESOURCE resourceType, int amount) {
        storedResources[resourceType] = amount;
        storedResources[resourceType] = Mathf.Clamp(storedResources[resourceType], 0, maxResourceValues[resourceType]);
    }
    public void AdjustResource(RESOURCE resourceType, int amount) {
        storedResources[resourceType] += amount;
        storedResources[resourceType] = Mathf.Clamp(storedResources[resourceType], 0, maxResourceValues[resourceType]);
    }
    public bool HasResourceAmount(RESOURCE resourceType, int amount) {
        return storedResources[resourceType] >= amount;
    }
    public bool IsAtMaxResource(RESOURCE resource) {
        return storedResources[resource] >= maxResourceValues[resource];
    }
    public bool HasEnoughSpaceFor(RESOURCE resource, int amount) {
        int newAmount = storedResources[resource] + amount;
        return newAmount <= maxResourceValues[resource];
    }
    #endregion

    public override string ToString() {
        return $"{name} {id.ToString()}";
    }

    #region Player Action Target
    public List<PlayerAction> actions { get; private set; }
    public void ConstructDefaultActions() {
        actions = new List<PlayerAction>();
        
        PlayerAction destroyAction = new PlayerAction("Destroy", 
            () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.DESTROY].CanPerformAbilityTowards(this),
            () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.DESTROY].ActivateAbility(this));
        PlayerAction igniteAction = new PlayerAction("Ignite", 
            () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.IGNITE].CanPerformAbilityTowards(this), 
            () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.IGNITE].ActivateAbility(this));
        PlayerAction poisonAction = new PlayerAction("Poison", 
            () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.SPOIL].CanPerformAbilityTowards(this), 
            () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.SPOIL].ActivateAbility(this));
        PlayerAction animateAction = new PlayerAction("Animate", () => false, null);
        PlayerAction seizeAction = new PlayerAction("Seize", 
            () => !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && this.mapVisual != null && (this.isBeingCarriedBy != null || this.gridTileLocation != null), 
             () => PlayerManager.Instance.player.seizeComponent.SeizePOI(this));
        
        AddPlayerAction(destroyAction);
        AddPlayerAction(igniteAction);
        AddPlayerAction(poisonAction);
        AddPlayerAction(animateAction);
        AddPlayerAction(seizeAction);
    }
    public void AddPlayerAction(PlayerAction action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(Signals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
        }
    }
    public void RemovePlayerAction(PlayerAction action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void ClearPlayerActions() {
        actions.Clear();
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
            structureLocationAreaID = tileObject.structureLocation.location.id; //TODO: Refactor, because location is no longer guaranteed to be an settlement.
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

        //awareCharactersIDs = new List<int>();
        //for (int i = 0; i < tileObject.awareCharacters.Count; i++) {
        //    awareCharactersIDs.Add(tileObject.awareCharacters[i].id);
        //}
    }

    public virtual TileObject Load() {
        string tileObjectName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObjectType.ToString());
        TileObject tileObject = System.Activator.CreateInstance(System.Type.GetType(tileObjectName), this) as TileObject;

        //if(structureLocationID != -1 && structureLocationAreaID != -1) {
        //    Settlement settlement = LandmarkManager.Instance.GetAreaByID(structureLocationAreaID);
        //    tileObject.SetStructureLocation(settlement.GetStructureByID(structureLocationType, structureLocationID));
        //}
        //for (int i = 0; i < awareCharactersIDs.Count; i++) {
        //    tileObject.AddAwareCharacter(CharacterManager.Instance.GetCharacterByID(awareCharactersIDs[i]));
        //}

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
            Settlement settlement = LandmarkManager.Instance.GetAreaByID(previousTileAreaID);
            LocationGridTile tile = settlement.innerMap.map[(int)previousTileID.x, (int)previousTileID.y];
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