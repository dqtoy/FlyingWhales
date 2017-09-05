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
    private int _likeFromMutualRelationships;
    private string _relationshipSummary;
    private string _relationshipEventsSummary;
    private bool _isInitial;
    private bool _isSharingBorder;

    private bool _isAtWar;
	private bool _isAdjacent;
    private War _war;
    private InvasionPlan _invasionPlan;
    private RequestPeace _requestPeace;
    private KingdomWar _kingdomWarData;
    private GameDate _requestPeaceCooldown;

	private bool _isMilitaryAlliance;
	private bool _isMutualDefenseTreaty;

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
        get { return _like + _eventLikenessModifier + _likeFromMutualRelationships; }
    }
    public int eventLikenessModifier {
        get { return _eventLikenessModifier; }
    }
    public int likeFromMutualRelationships {
        get { return _likeFromMutualRelationships; }
    }
    public bool isAtWar {
        get { return _isAtWar; }
    }
    public War war {
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
	public bool isMilitaryAlliance {
		get { return this._isMilitaryAlliance; }
	}
	public bool isMutualDefenseTreaty {
		get { return this._isMutualDefenseTreaty; }
	}
	public bool isAdjacent {
		get { return this._isAdjacent; }
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
        _likeFromMutualRelationships = 0;
        _relationshipSummary = string.Empty;
        _relationshipEventsSummary = string.Empty;
        _isInitial = false;
        _kingdomWarData = new KingdomWar(_targetKingdom);
        _requestPeaceCooldown = new GameDate(0,0,0);
		this._isMilitaryAlliance = false;
		this._isMutualDefenseTreaty = false;
		this._isAdjacent = false;
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
            _relationshipStatus = RELATIONSHIP_STATUS.RIVAL;
        } else if (totalLike >= -80 && totalLike <= -41) {
            _relationshipStatus = RELATIONSHIP_STATUS.ENEMY;
        } else if (totalLike >= -40 && totalLike <= -21) {
            _relationshipStatus = RELATIONSHIP_STATUS.COLD;
        } else if (totalLike >= -20 && totalLike <= 20) {
            _relationshipStatus = RELATIONSHIP_STATUS.NEUTRAL;
        } else if (totalLike >= 21 && totalLike <= 40) {
            _relationshipStatus = RELATIONSHIP_STATUS.WARM;
        } else if (totalLike >= 41 && totalLike <= 80) {
            _relationshipStatus = RELATIONSHIP_STATUS.FRIEND;
        } else {
            _relationshipStatus = RELATIONSHIP_STATUS.ALLY;
        }
        if (previousStatus != _relationshipStatus) {
            //relationship status changed
            _sourceKingdom.UpdateMutualRelationships();
        }
    }

    internal void ResetMutualRelationshipModifier() {
        _likeFromMutualRelationships = 0;
    }

    internal void AddMutualRelationshipModifier(int amount) {
        this._likeFromMutualRelationships += amount;
    }

    /*
     * <summary>
     * Update likeness based on set criteria (shared values, shared kingdom type, etc.)
     * </summary>
     * */
    internal void UpdateLikeness(GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        this._relationshipSummary = string.Empty;
        int baseLoyalty = 0;
        int adjustment = 0;

        KingdomRelationship relationshipKingdom = _sourceKingdom.GetRelationshipWithKingdom(_targetKingdom);

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
            this._relationshipSummary += adjustment.ToString() + "   kingdom type.\n";
        }


        //At War
        if (relationshipKingdom != null) {
            if (relationshipKingdom.isAtWar) {
                adjustment = -30;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + "   at war.\n";
            }
        }

        //Race
        if (_sourceKingdom.race != _targetKingdom.race && !sourceKingValues.Contains(CHARACTER_VALUE.EQUALITY)) {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + "   different race.\n";
        }

        //Sharing Border
        if (relationshipKingdom.isSharingBorder) {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + "   shared borders.\n";
        }

        //Values
        List<CHARACTER_VALUE> valuesInCommonExceptInfluence = valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).ToList();
        if (valuesInCommonExceptInfluence.Count == 1) {
            adjustment = 0;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + "   shared values.\n";
        } else if (valuesInCommonExceptInfluence.Count == 2) {
            adjustment = 15;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + "   shared values.\n";
        } else if (valuesInCommonExceptInfluence.Count >= 3) {
            adjustment = 30;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + "   shared values.\n";
        } else {
            adjustment = -30;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + "   no shared values.\n";
        }

        if (sourceKingValues.Contains(CHARACTER_VALUE.PEACE)) {
            adjustment = 15;
            baseLoyalty += adjustment;
            this._relationshipSummary += "+" + adjustment.ToString() + "   values peace.\n";
        }

        if (sourceKingValues.Contains(CHARACTER_VALUE.DOMINATION)) {
            adjustment = -15;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + "   values domination.\n";
        }
        this._like = 0;
        this.AdjustLikeness(baseLoyalty, gameEventTrigger, assassinationReasons, isDiscovery);
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
        if (newStatus == RELATIONSHIP_STATUS.ALLY) {
            this._like = 81;
        } else if (newStatus == RELATIONSHIP_STATUS.FRIEND) {
            this._like = 41;
        } else if (newStatus == RELATIONSHIP_STATUS.WARM) {
            this._like = 21;
        } else if (newStatus == RELATIONSHIP_STATUS.NEUTRAL) {
            this._like = 0;
        } else if (newStatus == RELATIONSHIP_STATUS.COLD) {
            this._like = -21;
        } else if (newStatus == RELATIONSHIP_STATUS.ENEMY) {
            this._like = -41;
        } else if (newStatus == RELATIONSHIP_STATUS.RIVAL) {
            this._like = -81;
        }
        UpdateKingRelationshipStatus();
        if (this.relationshipStatus == RELATIONSHIP_STATUS.ENEMY || this.relationshipStatus == RELATIONSHIP_STATUS.RIVAL) {
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
            if (_relationshipStatus == RELATIONSHIP_STATUS.ENEMY || _relationshipStatus == RELATIONSHIP_STATUS.RIVAL) {
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
            if (previousRelationshipStatus == RELATIONSHIP_STATUS.ENEMY && _relationshipStatus == RELATIONSHIP_STATUS.COLD) {
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
    internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;

        GameDate dateTimeToUse = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        if (gameEventTrigger.eventType == EVENT_TYPES.KINGDOM_WAR) {
            dateTimeToUse.AddYears(1); //Relationship modifiers from war will take 1 year to expire
        } else {
            dateTimeToUse.AddMonths(3); //Relationship modifiers from all other events will take 3 months to expire
        }

        ExpirableModifier expMod = new ExpirableModifier(gameEventTrigger, summary, dateTimeToUse, modification);
        this._eventModifiers.Add(expMod);
        SchedulingManager.Instance.AddEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));

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
			UpdateKingRelationshipStatus();
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
            _invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
            _sourceKingdom.king, _sourceKingdom, _targetKingdom, gameEventTrigger, _war);
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
            this._war.GameEventWarWinner(this._targetKingdom);
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
                this._war.RequestPeace(this._sourceKingdom);
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

    internal void AssignWarEvent(War war) {
        _war = war;
    }

    internal void SetWarStatus(bool warStatus) {
        _isAtWar = warStatus;
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

	internal void ChangeMilitaryAlliance(bool state){
		this._isMilitaryAlliance = state;
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.ChangeMilitaryAlliance (state);
	}

	internal void ChangeMutualDefenseTreaty(bool state){
		this._isMutualDefenseTreaty = state;
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.ChangeMutualDefenseTreaty (state);
	}

	internal void ChangeAdjancency(bool state){
		this._isAdjacent = state;
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.ChangeAdjancency (state);
	}
}
