using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SpecialToken : IPointOfInterest {
    public string name { get; private set; }
    public SPECIAL_TOKEN specialTokenType;
    public int weight;
    public Faction owner;
    public Character characterOwner { get; private set; }
    public LocationStructure structureLocation { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public int supplyValue { get { return ItemManager.Instance.itemData[specialTokenType].supplyValue; } }
    public int craftCost { get { return ItemManager.Instance.itemData[specialTokenType].craftCost; } }
    public int purchaseCost { get { return ItemManager.Instance.itemData[specialTokenType].purchaseCost; } }
    public Area specificLocation { get { return gridTileLocation.structure.location; } }
    public bool isDisabledByPlayer { get; protected set; }
    public POI_STATE state { get; protected set; }
    public POICollisionTrigger collisionTrigger { get; protected set; }
    public int uses { get; protected set; } //how many times can this item be used?
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    public List<GoapAction> targettedByAction { get; protected set; }

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
    public List<Trait> normalTraits {
        get { return _traits; }
    }
    public Faction factionOwner {
        get { return owner; }
    }
    #endregion

    public SpecialToken(SPECIAL_TOKEN specialTokenType, int appearanceRate) : base() {
        this.specialTokenType = specialTokenType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.specialTokenType.ToString());
        weight = appearanceRate;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PICK_ITEM_GOAP, INTERACTION_TYPE.STEAL, INTERACTION_TYPE.SCRAP, INTERACTION_TYPE.ITEM_DESTROY, INTERACTION_TYPE.DROP_ITEM_HOME};
        _traits = new List<Trait>();
        allJobsTargettingThis = new List<JobQueueItem>();
        targettedByAction = new List<GoapAction>();
        uses = 1;
        InitializeCollisionTrigger();
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
    public bool HasJobTargettingThis(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType) {
                return true;
            }
        }
        return false;
    }
    public void AddTargettedByAction(GoapAction action) {
        targettedByAction.Add(action);
    }
    public void RemoveTargettedByAction(GoapAction action) {
        targettedByAction.Remove(action);
    }

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
        if (poiGoapActions != null && poiGoapActions.Count > 0 && gridTileLocation != null) { //only advertise items that are not being carried
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
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
        poiGoapActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        poiGoapActions.Add(type);
    }
    #endregion

    #region Traits
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (AttributeManager.Instance.IsInstancedTrait(traitName)) {
            return AddTrait(AttributeManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        }
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (trait.IsUnique()) {
            Trait oldTrait = GetNormalTrait(trait.name);
            if (oldTrait != null) {
                oldTrait.SetCharacterResponsibleForTrait(characterResponsible);
                oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                return false;
            }
        }
        _traits.Add(trait);
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        trait.AddCharacterResponsibleForTrait(characterResponsible);
        //ApplyTraitEffects(trait);
        //ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait), this);
            trait.SetExpiryTicket(this, ticket);
        }
        if (triggerOnAdd) {
            trait.OnAddTrait(this);
        }
        return true;
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null, bool includeAlterEgo = true) {
        if (_traits.Remove(trait)) {
            trait.RemoveExpiryTicket(this);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this, removedBy);
            }
            return true;
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true, Character removedBy = null) {
        Trait trait = GetNormalTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove, removedBy);
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
    public Trait GetNormalTrait(params string[] traitNames) {
        for (int i = 0; i < _traits.Count; i++) {
            Trait trait = _traits[i];

            for (int j = 0; j < traitNames.Length; j++) {
                if (trait.name == traitNames[j] && !trait.isDisabled) {
                    return trait;
                }
            }
        }
        return null;
    }
    private void RemoveAllTraits() {
        List<Trait> allTraits = new List<Trait>(normalTraits);
        for (int i = 0; i < allTraits.Count; i++) {
            RemoveTrait(allTraits[i]);
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