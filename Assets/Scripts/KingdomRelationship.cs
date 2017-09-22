using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class KingdomRelationship {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;
    private RELATIONSHIP_STATUS _relationshipStatus;
    private List<History> _relationshipHistory;
    private List<ExpirableModifier> _eventModifiers;
    private int _like;
    private int _eventLikenessModifier;
    public int forTestingLikeModifier;
    private string _relationshipSummary;
    private string _relationshipEventsSummary;
    private bool _isInitial;
    private bool _isSharingBorder;

    private bool _isAtWar;
	private bool _isAdjacent;
	private bool _isDiscovered;
    private Wars _war;
    private InvasionPlan _invasionPlan;
    private RequestPeace _requestPeace;
    private KingdomWar _kingdomWarData;
    private GameDate _requestPeaceCooldown;
    private MilitaryAllianceOffer _currentActiveMilitaryAllianceOffer;
    private MutualDefenseTreaty _currentActiveDefenseTreatyOffer;

	private bool _isMilitaryAlliance;
	private bool _isMutualDefenseTreaty;

	private Dictionary<EVENT_TYPES, bool> _eventBuffs;

	private GameDate _currentExpirationDefenseTreaty;
	private GameDate _currentExpirationMilitaryAlliance;

	//Kingdom Threat
	private float _racePercentageModifier;
	private float _targetKingdomThreatLevel;
	private float _targetKingdomInvasionValue;

	internal int _usedSourceEffectivePower;
	internal int _usedSourceEffectiveDef;
	internal int _usedTargetEffectivePower;
	internal int _usedTargetEffectiveDef;

    #region getters/setters
    public Kingdom sourceKingdom {
        get { return _sourceKingdom; }
    }
    public Kingdom targetKingdom {
        get { return _targetKingdom; }
    }
    public RELATIONSHIP_STATUS relationshipStatus {
        get { return _relationshipStatus; }
    }
    public List<ExpirableModifier> eventModifiers {
        get { return this._eventModifiers; }
    }
    public string relationshipSummary {
        get { return this._relationshipSummary + this._relationshipEventsSummary; }
    }
    public int totalLike {
        get { return _like + _eventLikenessModifier + forTestingLikeModifier; }
    }
    public int eventLikenessModifier {
        get { return _eventLikenessModifier; }
    }
    public bool isAtWar {
        get { return _isAtWar; }
    }
    public Wars war {
        get { return _war; }
    }
    public bool isSharingBorder {
        get { return _isSharingBorder; }
    }
    public KingdomWar kingdomWarData {
        get { return _kingdomWarData; }
    }
    public InvasionPlan invasionPlan {
        get { return _invasionPlan; }
    }
    public RequestPeace requestPeace {
        get { return _requestPeace; }
    }
    public GameDate requestPeaceCooldown {
        get { return _requestPeaceCooldown; }
    }
    public MilitaryAllianceOffer currentActiveMilitaryAllianceOffer {
        get { return _currentActiveMilitaryAllianceOffer; }
        set {
            if(_currentActiveMilitaryAllianceOffer != null && _currentActiveMilitaryAllianceOffer.isActive) {
                throw new System.Exception (_sourceKingdom.name + " still has an active military alliance offer with " + _targetKingdom.name);
            } else {
                _currentActiveMilitaryAllianceOffer = value;
            }
        }
    }
    public MutualDefenseTreaty currentActiveDefenseTreatyOffer {
        get { return _currentActiveDefenseTreatyOffer; }
        set {
            if (_currentActiveDefenseTreatyOffer != null && _currentActiveDefenseTreatyOffer.isActive) {
                throw new System.Exception(_sourceKingdom.name + " still has an active defense treaty offer with " + _targetKingdom.name);
            } else {
                _currentActiveDefenseTreatyOffer = value;
            }
        }
    }
    public bool isMilitaryAlliance {
		get { return this._isMilitaryAlliance; }
	}
	public bool isMutualDefenseTreaty {
		get { return this._isMutualDefenseTreaty; }
	}
	public bool isAdjacent {
        get { return this._isAdjacent; }
	}
	public Dictionary<EVENT_TYPES, bool> eventBuffs {
		get { return this._eventBuffs; }
	}
	public float targetKingdomThreatLevel{
		get {
			return this._targetKingdomThreatLevel;
		}
	}
	public float targetKingdomInvasionValue{
		get {
			return this._targetKingdomInvasionValue;
		}
	}
	public bool isDiscovered {
		get { return this._isDiscovered; }
	}
    #endregion

    /* <summary> Create a new relationship between 2 kingdoms </summary>
     * <param name="sourceKingdom">Kingdom that this relationship object belongs to.</param> 
     * <param name="targetKingdom">Kingdom that this relationship is aimed towards.</param>
     * */
    public KingdomRelationship(Kingdom _sourceKingdom, Kingdom _targetKingdom) {
        this._sourceKingdom = _sourceKingdom;
        this._targetKingdom = _targetKingdom;
        _relationshipHistory = new List<History>();
        _eventModifiers = new List<ExpirableModifier>();
        _like = 0;
        _eventLikenessModifier = 0;
        _relationshipSummary = string.Empty;
        _relationshipEventsSummary = string.Empty;
        _isInitial = false;
        _kingdomWarData = new KingdomWar(_targetKingdom);
        _requestPeaceCooldown = new GameDate(0,0,0);
		this._isMilitaryAlliance = false;
		this._isMutualDefenseTreaty = false;
		this._isAdjacent = false;
		this._currentExpirationDefenseTreaty = new GameDate (0, 0, 0);
		this._currentExpirationMilitaryAlliance = new GameDate (0, 0, 0);

		this._eventBuffs = new Dictionary<EVENT_TYPES, bool>(){
			{EVENT_TYPES.TRIBUTE, false},
			{EVENT_TYPES.INSTIGATION, false},
		};

		SetRaceThreatModifier ();
        //UpdateLikeness(null);
    }

    /*
     * <summary>
     * Force change the base like value of this relationship
     * NOTE: This will automatically update relationshipStatus
     * </summary>
     * */
    internal void SetLikeness(int likeness) {
        _like = likeness;
        UpdateKingRelationshipStatus();
    }

    /*
     * <summary>
     * Update relationshipStatus based on total likeness.
     * </summary>
     * */
    internal void UpdateKingRelationshipStatus() {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
        if (totalLike <= -81) {
            _relationshipStatus = RELATIONSHIP_STATUS.SPITE;
        } else if (totalLike >= -80 && totalLike <= -41) {
            _relationshipStatus = RELATIONSHIP_STATUS.HATE;
        } else if (totalLike >= -40 && totalLike <= -21) {
            _relationshipStatus = RELATIONSHIP_STATUS.DISLIKE;
        } else if (totalLike >= -20 && totalLike <= 20) {
            _relationshipStatus = RELATIONSHIP_STATUS.NEUTRAL;
        } else if (totalLike >= 21 && totalLike <= 40) {
            _relationshipStatus = RELATIONSHIP_STATUS.LIKE;
        } else if (totalLike >= 41 && totalLike <= 80) {
            _relationshipStatus = RELATIONSHIP_STATUS.AFFECTIONATE;
        } else {
            _relationshipStatus = RELATIONSHIP_STATUS.LOVE;
        }
        //if (previousStatus != _relationshipStatus) {
        //    //relationship status changed
        //    _sourceKingdom.UpdateMutualRelationships();
        //}
    }

    //internal void ResetMutualRelationshipModifier() {
    //    _likeFromMutualRelationships = 0;
    //}

    //internal void AddMutualRelationshipModifier(int amount) {
    //    this._likeFromMutualRelationships += amount;
    //}

    /*
     * <summary>
     * Update likeness based on set criteria (shared values, shared kingdom type, etc.)
     * </summary>
     * */
    internal void UpdateLikeness(GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        this._relationshipSummary = string.Empty;
        int baseLoyalty = 0;
        int adjustment = 0;

        List<CHARACTER_VALUE> sourceKingValues = _sourceKingdom.king.importantCharacterValues.Select(x => x.Key).ToList();
        List<CHARACTER_VALUE> targetKingValues = _targetKingdom.king.importantCharacterValues.Select(x => x.Key).ToList();

        List<CHARACTER_VALUE> valuesInCommon = sourceKingValues.Intersect(targetKingValues).ToList();

        //Kingdom Type
        if (_sourceKingdom.kingdomTypeData.dictRelationshipKingdomType.ContainsKey(_targetKingdom.kingdomType)) {
            adjustment = _sourceKingdom.kingdomTypeData.dictRelationshipKingdomType[_targetKingdom.kingdomType];
            baseLoyalty += adjustment;
            if (adjustment >= 0) {
                this._relationshipSummary += "+";
            }
            this._relationshipSummary += adjustment.ToString() + " kingdom type.\n";
        }


        //At War
//		if (this.isAtWar) {
//			adjustment = -30;
//			baseLoyalty += adjustment;
//			this._relationshipSummary += adjustment.ToString() + "   at war.\n";
//		}

        //Race
        if (_sourceKingdom.race != _targetKingdom.race && !sourceKingValues.Contains(CHARACTER_VALUE.EQUALITY)) {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + " different race.\n";
        }

        //Sharing Border
		if (this.isSharingBorder) {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + " shared borders.\n";
        }

        //Values
        List<CHARACTER_VALUE> valuesInCommonExceptInfluence = valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).ToList();
        if (valuesInCommonExceptInfluence.Count == 1) {
            adjustment = 5;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + " shared values.\n";
        } else if (valuesInCommonExceptInfluence.Count == 2) {
            adjustment = 15;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + " shared values.\n";
        } else if (valuesInCommonExceptInfluence.Count >= 3) {
            adjustment = 30;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + " shared values.\n";
        } else {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + " no shared values.\n";
        }

        if (sourceKingValues.Contains(CHARACTER_VALUE.PEACE)) {
            adjustment = 15;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + " values peace.\n";
        }

        if (sourceKingValues.Contains(CHARACTER_VALUE.DOMINATION)) {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + " values domination.\n";
        }

		if(this._targetKingdomThreatLevel == 0){
			adjustment = 25;
		}else if(this._targetKingdomThreatLevel >= 1 && this._targetKingdomThreatLevel < 20){
			adjustment = 0;
		}else if(this._targetKingdomThreatLevel >= 20 && this._targetKingdomThreatLevel < 50){
			adjustment = -25;
		}else if(this._targetKingdomThreatLevel >= 50 && this._targetKingdomThreatLevel < 100){
			adjustment = -50;
		}else{
			adjustment = -100;
		}
		baseLoyalty += adjustment;
		this._relationshipSummary += adjustment.ToString() + " kingdom threat.\n";

//		//Lacks Prestige
//		if(this.targetKingdom.doesLackPrestige){
//			adjustment = -30;
//			baseLoyalty += adjustment;
//			this._relationshipSummary += adjustment.ToString() + "   lacks prestige.\n";
//		}
        this._like = 0;
        this.AdjustLikeness(baseLoyalty, gameEventTrigger, assassinationReasons, isDiscovery);
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom == _sourceKingdom) {
            UIManager.Instance.UpdateRelationships();
        }
    }

    /*
     * <summary>
     * Adjust likeness by an amount, relative to the current base likeness.
     * </summary>
     * <param name="adjustment"> Amount of likeness to add/subtract (Specify with +/-) </param>
     * <param name="gameEventTrigger"> Game Event that triggered change in relationship </param>
     * <param name="assassinationReasons"> Assassination Reason (Default to NONE) </param>
     * <param name="isDiscovery"> Is from discovery? (Default is false) </param>
     * */
    internal void AdjustLikeness(int adjustment, GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
        this._like += adjustment;
        this._like = Mathf.Clamp(this.totalLike, -100, 100);
        this.UpdateKingRelationshipStatus();

        if (!this._isInitial) {
            if (this.totalLike < 0) { //Relationship deteriorated
                _sourceKingdom.OnRelationshipDeteriorated(this, gameEventTrigger, isDiscovery, assassinationReasons);
                this.CheckForEmbargo(previousStatus, gameEventTrigger);
            } else { //Relationship improved
                _sourceKingdom.OnRelationshipImproved(this);
                this.CheckForDisembargo(previousStatus);
            }
        } else {
            this._isInitial = false;
        }
    }

    internal void ChangeRelationshipStatus(RELATIONSHIP_STATUS newStatus, GameEvent gameEventTrigger = null) {
        if (newStatus == RELATIONSHIP_STATUS.LOVE) {
            this._like = 81;
        } else if (newStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
            this._like = 41;
        } else if (newStatus == RELATIONSHIP_STATUS.LIKE) {
            this._like = 21;
        } else if (newStatus == RELATIONSHIP_STATUS.NEUTRAL) {
            this._like = 0;
        } else if (newStatus == RELATIONSHIP_STATUS.DISLIKE) {
            this._like = -21;
        } else if (newStatus == RELATIONSHIP_STATUS.HATE) {
            this._like = -41;
        } else if (newStatus == RELATIONSHIP_STATUS.SPITE) {
            this._like = -81;
        }
        UpdateKingRelationshipStatus();
        if (this.relationshipStatus == RELATIONSHIP_STATUS.HATE || this.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
            Embargo(gameEventTrigger);
        }
    }

    /*
     * <summary>
     * Set if sourceKingdom and targetKingdom are sharing borders
     * </summary>
     * */
    internal void SetBorderSharing(bool isSharingBorder) {
        _isSharingBorder = isSharingBorder;
        UpdateLikeness(null);
    }

    #region Trading
    /*
     * <summary>
     * This function checks if the targetKingdom is to be embargoed,
     * </summary>
     * */
    protected void CheckForEmbargo(RELATIONSHIP_STATUS previousRelationshipStatus, GameEvent gameEventTrigger) {
        if (previousRelationshipStatus != _relationshipStatus) { // Check if the relationship between the 2 kings changed in status
            //Check if the source kings relationship with king has changed to enemy or rival, if so, put king's kingdom in source king's embargo list
            if (_relationshipStatus == RELATIONSHIP_STATUS.HATE || _relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
                Embargo(gameEventTrigger);
            }
        }
    }

    /*
     * <summary>
     * Put targetKing's kingdom in sourceKing's kingdom embargo list.
     * TODO: Add embargo reason from gameEventReasonForEmbargo when adding
     * kingdom to embargo list.
     * </summary>
     * */
    protected void Embargo(GameEvent gameEventReasonForEmbargo) {
        _sourceKingdom.AddKingdomToEmbargoList(_targetKingdom);
        //Debug.LogError(this.sourceKing.city.kingdom.name + " put " + this.king.city.kingdom.name + " in it's embargo list, beacuase of " + gameEventReasonForEmbargo.eventType.ToString());
    }

    /*
     * <summary>
     * This function checks if the targetKingdom is to be disembargoed,
     * requirement/s for disembargo:
     * - Relationship should go from ENEMY to COLD.
     * </summary>
     * */
    protected void CheckForDisembargo(RELATIONSHIP_STATUS previousRelationshipStatus) {
        if (previousRelationshipStatus != _relationshipStatus) { // Check if the relationship between the 2 kings changed in status
            //if the relationship changed from enemy to cold, disembargo the targetKing
            if (previousRelationshipStatus == RELATIONSHIP_STATUS.HATE && _relationshipStatus == RELATIONSHIP_STATUS.DISLIKE) {
                Disembargo();
            }
        }
    }

    /*
     * <summary>
     * Remove targetKingdom from sourceKingdom's embargo list
     * </summary>
     * */
    protected void Disembargo() {
        _sourceKingdom.RemoveKingdomFromEmbargoList(_targetKingdom);
        //Debug.LogError(_sourceKingdom.name + " removed " + _targetKingdom.name + " from it's embargo list!");
    }
    #endregion

    #region Event Modifiers
    /*
     * <summary>
     * Add an event modifier (A relationship modifier that came from a specific event that involved the sourceKingdom and targetKingdom)
     * </summary>
     * */
	internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger = null, bool hasExpiration = true, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
		bool hasAddedModifier = false;
		GameDate dateTimeToUse = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		if(gameEventTrigger != null){
			if (gameEventTrigger.eventType == EVENT_TYPES.KINGDOM_WAR) {
				dateTimeToUse.AddYears(1); //Relationship modifiers from war will take 1 year to expire
			} else {
				dateTimeToUse.AddMonths(3); //Relationship modifiers from all other events will take 3 months to expire
			}
		}else{
			dateTimeToUse.AddMonths(3);
		}

		if(gameEventTrigger != null){
			if(this.eventBuffs.ContainsKey(gameEventTrigger.eventType)){
				if(this.eventBuffs[gameEventTrigger.eventType]){
					for (int i = 0; i < this._eventModifiers.Count; i++) {
						ExpirableModifier expMod = this._eventModifiers [i];
						if(expMod.modifierGameEvent.eventType == gameEventTrigger.eventType){
							expMod.SetModifier (expMod.modifier + modification);

							if(hasExpiration){
								SchedulingManager.Instance.RemoveSpecificEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
								expMod.SetDueDate (dateTimeToUse);
								SchedulingManager.Instance.AddEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
							}

							hasAddedModifier = true;
							break;
						}
					}
				}else{
					this.eventBuffs[gameEventTrigger.eventType] = true;
				}
			}
		}

		if(!hasAddedModifier){
			ExpirableModifier expMod = new ExpirableModifier(gameEventTrigger, summary, dateTimeToUse, modification);
			this._eventModifiers.Add(expMod);
			if(hasExpiration){
				SchedulingManager.Instance.AddEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
			}
		}

        this._eventLikenessModifier += modification;
        if (modification < 0) {
            //Deteriorate Relationship
            _sourceKingdom.OnRelationshipDeteriorated(this, gameEventTrigger, isDiscovery, assassinationReasons);
            CheckForEmbargo(previousStatus, gameEventTrigger);
            if (gameEventTrigger != null && gameEventTrigger.eventType != EVENT_TYPES.KINGDOM_WAR) {
                 _sourceKingdom.WarTrigger(this, gameEventTrigger, _sourceKingdom.kingdomTypeData, gameEventTrigger.warTrigger);
            }
        } else {
            //Improve Relationship
            _sourceKingdom.OnRelationshipImproved(this);
            CheckForDisembargo(previousStatus);
        }
        UpdateKingRelationshipStatus();
    }

    /*
     * <summary>
     * Remove a specific event modifier
     * </summary>
     * */
    private void RemoveEventModifier(ExpirableModifier expMod) {
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
			if(expMod.modifier < 0) {
				//if the modifier is negative, return the previously subtracted value
				_eventLikenessModifier += Mathf.Abs(expMod.modifier);
			} else {
				//if the modifier is positive, subtract the amount that was previously added
				_eventLikenessModifier -= expMod.modifier;
			}
			_eventModifiers.Remove(expMod);
			if(expMod.modifierGameEvent != null){
				if(this._eventBuffs.ContainsKey(expMod.modifierGameEvent.eventType)){
					this._eventBuffs [expMod.modifierGameEvent.eventType] = false;
				}
			}
			UpdateKingRelationshipStatus();
		}
        
    }
	internal void RemoveEventModifierBySummary(string summary){
		for (int i = 0; i < this._eventModifiers.Count; i++) {
			ExpirableModifier expMod = this._eventModifiers[i];
			if(expMod.summary == summary){
				if(expMod.modifier < 0) {
					//if the modifier is negative, return the previously subtracted value
					_eventLikenessModifier += Mathf.Abs(expMod.modifier);
				} else {
					//if the modifier is positive, subtract the amount that was previously added
					_eventLikenessModifier -= expMod.modifier;
				}
				if(expMod.modifierGameEvent != null){
					if(this._eventBuffs.ContainsKey(expMod.modifierGameEvent.eventType)){
						this._eventBuffs [expMod.modifierGameEvent.eventType] = false;
					}
				}
				UpdateKingRelationshipStatus();
				_eventModifiers.RemoveAt(i);
				break;
			}
		}
	}
    /*
     * <summary>
     * Reset all Event Modifiers for this relationship
     * </summary>
     * */
    internal void ResetEventModifiers() {
        this._eventLikenessModifier = 0;
        this._eventModifiers.Clear();
        UpdateKingRelationshipStatus();
    }
    #endregion

    #region War Functions
    /*
     * <summary>
     * Create an invasion plan against targetKingdom
     * NOTE: sourceKingdom and targetKingdom must already have a war event between them for this to work
     * </summary>
     * */
    internal void CreateInvasionPlan(GameEvent gameEventTrigger) {
        if (_invasionPlan == null) {
//            _invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
//            _sourceKingdom.king, _sourceKingdom, _targetKingdom, gameEventTrigger, _war);
        } else {
            throw new Exception (_sourceKingdom.name + " already has an invasion plan towards " + _targetKingdom.name);
        }
    }

    internal void AssignRequestPeaceEvent(RequestPeace rp) {
        _requestPeace = rp;
    }

    /*
     * <summary>
     * Declare peace between sourceKingdom and targetKingdom
     * </summary>
     * */
    internal void DeclarePeace() {
        if (this._invasionPlan != null && this._invasionPlan.isActive) {
            this._invasionPlan.CancelEvent();
            this._invasionPlan = null;
        }

        if (this._requestPeace != null && this._requestPeace.isActive) {
//            this._war.GameEventWarWinner(this._targetKingdom);
            this._requestPeace.CancelEvent();
            this._requestPeace = null;
        }
        this._war = null;
        this._isAtWar = false;
    }

    internal void TriggerRequestPeace() {
        this._kingdomWarData.citiesLost += 1;
        if (this._sourceKingdom.cities.Count > 1 && _requestPeaceCooldown.month == 0) {
            int peaceValue = 20;
            int chance = UnityEngine.Random.Range(0, 100);
            if (this._war != null && chance < (peaceValue * this._kingdomWarData.citiesLost)) {
                //Request Peace
//                this._war.RequestPeace(this._sourceKingdom);
            }
        }
    }

    /*
     * <summary>
     * Queue sourceKingdom for request peace cooldown.
     * A kingdom can only request peace once every 3 months
     * </summary>
     * */
    internal void SetMoveOnPeriodAfterRequestPeaceRejection() {
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
	        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
	        dueDate.AddMonths(3);
	        _requestPeaceCooldown = dueDate;
	        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => ResetRequestPeaceCooldown());
		}
    }

    /*
     * <summary>
     * Reset Request Peace Cooldown
     * </summary>
     * */
    private void ResetRequestPeaceCooldown() {
        _requestPeaceCooldown = new GameDate(0,0,0);
    }

    internal void AssignWarEvent(Wars war) {
        _war = war;
    }

    internal void SetWarStatus(bool warStatus) {
		if(this._isAtWar != warStatus){
			this._isAtWar = warStatus;
		}
    }
	internal void SetDiscovery(bool state) {
		if(this._isDiscovered != state){
			this._isDiscovered = state;
		}
	}

    internal void AdjustExhaustion(int amount) {
        if (_isAtWar) {
            _kingdomWarData.AdjustExhaustion(amount);
        }
    }
    #endregion

    #region Relationship History
    internal void AddRelationshipHistory(History relHistory) {
        _relationshipHistory.Add(relHistory);
    }
    #endregion
	internal bool ChangeMilitaryAlliance(bool state){
		bool hasSourceChanged = AdjustMilitaryAlliance (state);
		KingdomRelationship targetRelationship = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		bool hasTargetChanged = targetRelationship.AdjustMilitaryAlliance (state);
		if(hasSourceChanged && hasTargetChanged && state){
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddYears (1);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.month, gameDate.year, () => MilitaryAllianceExpiration ());
			this._currentExpirationMilitaryAlliance.SetDate(gameDate);
		}
		return hasSourceChanged;
	}
	private bool AdjustMilitaryAlliance(bool state){
		if(this._isMilitaryAlliance != state){
			this._isMilitaryAlliance = state;
			if(state){
				this._sourceKingdom.AddMilitaryAlliance (this._targetKingdom);
			}else{
				this._sourceKingdom.RemoveMilitaryAlliance (this._targetKingdom);
			}
			return true;
		}else{
			return false;
		}
	}

	internal bool ChangeMutualDefenseTreaty(bool state){
		bool hasSourceChanged = AdjustMutualDefenseTreaty (state);
		KingdomRelationship targetRelationship = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		bool hasTargetChanged = targetRelationship.AdjustMutualDefenseTreaty (state);
		if(hasSourceChanged && hasTargetChanged && state){
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddYears (1);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.month, gameDate.year, () => DefenseTreatyExpiration ());
			this._currentExpirationDefenseTreaty.SetDate(gameDate);
		}
		return hasSourceChanged;
	}
	private bool AdjustMutualDefenseTreaty(bool state){
		if (this._isMutualDefenseTreaty != state) {
			this._isMutualDefenseTreaty = state;
			if (state) {
				this._sourceKingdom.AddMutualDefenseTreaty (this._targetKingdom);
			} else {
				this._sourceKingdom.RemoveMutualDefenseTreaty (this._targetKingdom);
			}
			return true;
		}else{
			return false;
		}
	}

	internal void ChangeAdjacency(bool state){
		AdjustAdjacency (state);
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.AdjustAdjacency (state);
	}
	private void AdjustAdjacency(bool state){
		if(this._isAdjacent != state){
			this._isAdjacent = state;
            UpdateTargetInvasionValue();
            UpdateTargetKingdomThreatLevel();
            if (state){
				this._sourceKingdom.AddAdjacentKingdom (this._targetKingdom);
			}else{
				this._sourceKingdom.RemoveAdjacentKingdom (this._targetKingdom);
			}
		}
	}

	internal void ChangeWarStatus(bool state){
		SetWarStatus(state);
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.SetWarStatus(state);
	}
	internal void ChangeDiscovery(bool state){
		SetDiscovery(state);
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.SetDiscovery(state);
	}
	private void DefenseTreatyExpiration(){
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead && this._isMutualDefenseTreaty 
			&& this._currentExpirationDefenseTreaty.IsSameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year)){

			ChangeMutualDefenseTreaty (false);
			RenewDefenseTreaty ();

		}
	}
	private void RenewDefenseTreaty(){
		bool isSourceWillingToRenew = this._sourceKingdom.RenewMutualDefenseTreatyWith (this._targetKingdom, this);
		if(isSourceWillingToRenew){
			ChangeMutualDefenseTreaty (true);
		}
	}

	private void MilitaryAllianceExpiration(){
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead && this._isMilitaryAlliance 
			&& this._currentExpirationMilitaryAlliance.IsSameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year)){

			ChangeMilitaryAlliance (false);
			RenewMilitaryAlliance ();

		}
	}
	private void RenewMilitaryAlliance(){
		bool isSourceWillingToRenew = this._sourceKingdom.RenewMilitaryAllianceWith (this._targetKingdom, this);
		if(isSourceWillingToRenew){
			ChangeMilitaryAlliance (true);
		}
	}

	private void SetRaceThreatModifier(){
		if(this._sourceKingdom.race != this._targetKingdom.race){
			this._racePercentageModifier = 1.15f;
		}else{
			this._racePercentageModifier = 1f;
		}
	}
	internal void UpdateTargetKingdomThreatLevel(){

		//+1 for every percentage point of effective power above my effective defense (max 100)
		float threatLevel = 0f;

		this._usedTargetEffectivePower = this._targetKingdom.effectivePower;
		this._usedSourceEffectiveDef = this._sourceKingdom.effectiveDefense;


		if(this._targetKingdom.effectivePower > this._sourceKingdom.effectiveDefense){
			HexTile hexTile = CityGenerator.Instance.GetExpandableTileForKingdom (this._targetKingdom);
			if(hexTile == null){
				threatLevel = (((float)this._targetKingdom.effectivePower / (float)this._sourceKingdom.effectiveDefense) * 100f) - 100f;
				threatLevel = Mathf.Clamp (threatLevel, 0f, 100f);

				//if different race: +15%
				threatLevel *= this._racePercentageModifier;

				//if currently at war with someone else: -50%
				if(this._sourceKingdom.HasWar(this._targetKingdom)){
					threatLevel -= (threatLevel * 0.5f);
				}

				//if not at war but militarizing
				if(!this._isAtWar && this._targetKingdom.isMilitarize){
					threatLevel *= 1.25f;
				}

				//warmongering
				if(this._targetKingdom.warmongerValue < 25){
					threatLevel -= (threatLevel * 0.15f);
				}else if(this._targetKingdom.warmongerValue >= 25 && this._targetKingdom.warmongerValue < 50){
					threatLevel *= 1.05f;
				}else if(this._targetKingdom.warmongerValue >= 50 && this._targetKingdom.warmongerValue < 75){
					threatLevel *= 1.25f;
				}else{
					threatLevel *= 1.5f;
				}

				//adjacency
				if(!this._isAdjacent){
					threatLevel -= (threatLevel * 0.5f);
				}

				//cannot expand due to lack of prestige
				if(this._targetKingdom.doesLackPrestige){
					threatLevel -= (threatLevel * 0.5f);
				}
			}
		}

		this._targetKingdomThreatLevel = threatLevel;
		if(this._targetKingdomThreatLevel < 0){
			this._targetKingdomThreatLevel = 0;
		}
		UpdateLikeness (null);
	}

	internal void UpdateTargetInvasionValue(){
		float invasionValue = 0;

		this._usedSourceEffectivePower = this._sourceKingdom.effectivePower;
		this._usedTargetEffectiveDef = this._targetKingdom.effectiveDefense;

		if(this._sourceKingdom.effectivePower > this._targetKingdom.effectiveDefense){
			if (this.isAdjacent && !AreAllies ()) {
				//+1 for every percentage point of my effective power above his effective defense (no max cap)
				invasionValue = (((float)this._sourceKingdom.effectivePower / (float)this._targetKingdom.effectiveDefense) * 100f) - 100f;
				if (invasionValue < 0) {
					invasionValue = 0;
				}

				//+-% for every point of Opinion towards target
				float likePercent = (float)this.totalLike / 100f;
				invasionValue -= (likePercent * invasionValue);

				//if target is currently at war with someone else
				if (this._targetKingdom.HasWar (this._sourceKingdom)) {
					invasionValue *= 1.25f;
				}
			}
		}

		this._targetKingdomInvasionValue = invasionValue;
		if(this._targetKingdomInvasionValue < 0){
			this._targetKingdomInvasionValue = 0;
		}
		if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom == _sourceKingdom) {
			UIManager.Instance.UpdateRelationships();
		}
	}
	internal bool AreAllies(){
		if(this._sourceKingdom.alliancePool == null || this._targetKingdom.alliancePool == null){
			return false;
		}else{
			if(this._sourceKingdom.alliancePool.id != this._targetKingdom.alliancePool.id){
				return false;
			}
		}
		return true;
	}
}
