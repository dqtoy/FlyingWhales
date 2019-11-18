using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;

public class SpecialToken : IPointOfInterest {
    public int id { get; private set; }
    public string name { get; private set; }
    public SPECIAL_TOKEN specialTokenType;
    public int weight;
    public Faction owner;
    public Character characterOwner { get; private set; }
    public LocationStructure structureLocation { get; private set; }
    public List<INTERACTION_TYPE> advertisedActions { get; private set; }
    public int supplyValue { get { return TokenManager.Instance.itemData[specialTokenType].supplyValue; } }
    public int craftCost { get { return TokenManager.Instance.itemData[specialTokenType].craftCost; } }
    public int purchaseCost { get { return TokenManager.Instance.itemData[specialTokenType].purchaseCost; } }
    public Area specificLocation { get { return gridTileLocation.structure.location; } }
    public bool isDisabledByPlayer { get; protected set; }
    public POI_STATE state { get; protected set; }
    public POICollisionTrigger collisionTrigger { get; protected set; }
    public int uses { get; protected set; } //how many times can this item be used?
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    //public List<GoapAction> targettedByAction { get; protected set; }

    //hp
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }

    protected List<Trait> _traits;
    private LocationGridTile tile;

    #region getters/setters
    public string tokenName {
        get { return name; }
    }
    public virtual string Item_Used {
        get { return "Item Used"; }
    }
    public virtual string Stop_Fail {
        get { return "Stop Fail"; }
    }
    public string ownerName {
        get {
            if (owner == null) {
                return "no one";
            } else {
                return owner.name;
            }
        }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.ITEM; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public Faction factionOwner {
        get { return owner; }
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

    public SpecialToken(SPECIAL_TOKEN specialTokenType, int appearanceRate) : base() {
        id = Utilities.SetID(this);
        this.specialTokenType = specialTokenType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.specialTokenType.ToString());
        weight = appearanceRate;
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PICK_UP, INTERACTION_TYPE.STEAL, INTERACTION_TYPE.SCRAP, INTERACTION_TYPE.ITEM_DESTROY, INTERACTION_TYPE.DROP_ITEM};
        _traits = new List<Trait>();
        allJobsTargettingThis = new List<JobQueueItem>();
        //targettedByAction = new List<GoapAction>();
        uses = 1;
        CreateTraitContainer();
        InitializeCollisionTrigger();
    }
    public void SetID(int id) {
        id = Utilities.SetID(this, id);
    }

    #region Virtuals
    public virtual void OnObtainToken(Character character) { }
    public virtual void OnUnobtainToken(Character character) { }
    public virtual void OnConsumeToken(Character character) {
        uses -= 1;
    }
    #endregion

    public void SetOwner(Faction owner) {
        this.owner = owner;
    }
    public void SetCharacterOwner(Character characterOwner) {
        this.characterOwner = characterOwner;
    }
    public void SetStructureLocation(LocationStructure structureLocation) {
        this.structureLocation = structureLocation;
    }
    public override string ToString() {
        return name;
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
    //public void AddTargettedByAction(GoapAction action) {
    //    targettedByAction.Add(action);
    //}
    //public void RemoveTargettedByAction(GoapAction action) {
    //    targettedByAction.Remove(action);
    //}

    #region Area Map
    public void SetGridTileLocation(LocationGridTile tile) {
        this.tile = tile;
        if (tile == null) {
            DisableCollisionTrigger();
            Messenger.Broadcast<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, this, tile);
        } else {
            PlaceCollisionTriggerAt(tile);
            Messenger.Broadcast<SpecialToken, LocationGridTile>(Signals.ITEM_PLACED_ON_TILE, this, tile);
        }
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
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
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, Dictionary<INTERACTION_TYPE, object[]> otherData) {
        if (advertisedActions != null && advertisedActions.Count > 0 && gridTileLocation != null) { //only advertise items that are not being carried
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < advertisedActions.Count; i++) {
                INTERACTION_TYPE currType = advertisedActions[i];
                if (RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currType, actor, this, data)
                        && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)) {
                        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        if (goapAction != null) {
                            if (data != null) {
                                goapAction.InitializeOtherData(data);
                            }
                            usableActions.Add(goapAction);
                        } else {
                            throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        }
                    }
                }
            }
            return usableActions;
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        this.state = state;
    }
    public bool IsAvailable() {
        return state != POI_STATE.INACTIVE && !isDisabledByPlayer;
    }
    public void SetIsDisabledByPlayer(bool state) {
        isDisabledByPlayer = state;
    }
    public void AddAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Add(type);
    }
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
    public bool IsValidCombatTarget() {
        return gridTileLocation != null;
    }
    #endregion

    #region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor { get { return TraitManager.specialTokenTraitProcessor; } }
    private void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    private void RemoveAllTraits() {
        List<Trait> allTraits = new List<Trait>(traitContainer.allTraits);
        for (int i = 0; i < allTraits.Count; i++) {
            traitContainer.RemoveTrait(this, allTraits[i]);
        }
    }
    #endregion

    #region Collision
    public void InitializeCollisionTrigger() {
        GameObject collisionGO = GameObject.Instantiate(InteriorMapManager.Instance.poiCollisionTriggerPrefab, InteriorMapManager.Instance.transform);
        SetCollisionTrigger(collisionGO.GetComponent<POICollisionTrigger>());
        collisionGO.SetActive(false);
        collisionTrigger.Initialize(this);
        RectTransform rt = collisionGO.transform as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
    }
    public void PlaceCollisionTriggerAt(LocationGridTile tile) {
        collisionTrigger.transform.SetParent(tile.parentAreaMap.objectsParent);
        (collisionTrigger.transform as RectTransform).anchoredPosition = tile.centeredLocalLocation;
        collisionTrigger.gameObject.SetActive(true);
        collisionTrigger.SetLocation(tile);
    }
    public void DisableCollisionTrigger() {
        collisionTrigger.gameObject.SetActive(false);
    }
    public void SetCollisionTrigger(POICollisionTrigger trigger) {
        collisionTrigger = trigger;
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

    #region Utilities
    public void DoCleanup() {
        RemoveAllTraits();
    }
    #endregion
}