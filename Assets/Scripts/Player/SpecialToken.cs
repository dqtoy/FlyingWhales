﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;

public class SpecialToken : AreaMapObject<SpecialToken>, IPointOfInterest {
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
    public int uses { get; protected set; } //how many times can this item be used?
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    public Character carriedByCharacter { get; private set; }

    //hp
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }

    private LocationGridTile tile;

    #region getters/setters
    public string tokenName {
        get { return name; }
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
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PICK_UP, INTERACTION_TYPE.STEAL, INTERACTION_TYPE.SCRAP, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.DROP_ITEM};
        allJobsTargettingThis = new List<JobQueueItem>();
        maxHP = 50;
        currentHP = maxHP;
        uses = 1;
        CreateTraitContainer();
        InitializeMapObject(this);
    }
    public void SetID(int id) {
        this.id = Utilities.SetID(this, id);
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
    public void SetCarriedByCharacter(Character character) {
        this.carriedByCharacter = character;
    }

    #region Area Map
    public void SetGridTileLocation(LocationGridTile tile) {
        this.tile = tile;
        if (tile == null) {
            DisableGameObject();
            Messenger.Broadcast<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, this, tile);
        } else {
            PlaceMapObjectAt(tile);
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
    //Returns the chosen action for the plan
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost) {
        GoapAction chosenAction = null;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isAvailable = IsAvailable();
            //List<GoapAction> usableActions = new List<GoapAction>();
            GoapAction lowestCostAction = null;
            int currentLowestCost = 0;
            for (int i = 0; i < advertisedActions.Count; i++) {
                INTERACTION_TYPE currType = advertisedActions[i];
                GoapAction action = InteractionManager.Instance.goapActionData[currType];
                if (!isAvailable && !action.canBeAdvertisedEvenIfActorIsUnavailable) {
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

    #region Utilities
    public void DoCleanup() {
        RemoveAllTraits();
    }
    #endregion

    #region Map Object
    protected override void CreateAreaMapGameObject() {
        GameObject obj = InteriorMapManager.Instance.areaMapObjectFactory.CreateNewItemAreaMapObject(this.poiType);
        areaMapGameObject = obj.GetComponent<ItemGameObject>();
    }
    protected override void OnMapObjectStateChanged() {
        if (mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            areaMapGameObject.SetVisualAlpha(128f / 255f);
            //remove all other interactions
            advertisedActions = new List<INTERACTION_TYPE>();

            AddAdvertisedAction(INTERACTION_TYPE.CRAFT_ITEM);
        } else {
            areaMapGameObject.SetVisualAlpha(255f / 255f);
            RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_ITEM);
            //restore default interactions
            advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PICK_UP, INTERACTION_TYPE.STEAL, INTERACTION_TYPE.SCRAP, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.DROP_ITEM };
            Messenger.Broadcast(Signals.ITEM_BUILT, this);
        }
    }
    #endregion
}