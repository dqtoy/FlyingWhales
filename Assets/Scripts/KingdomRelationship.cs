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
	private Dictionary<RELATIONSHIP_MODIFIER, RelationshipModifier> _relationshipModifiers;

    private int _like;
    private int _eventLikenessModifier;
    public int forTestingLikeModifier;
    private string _relationshipSummary;
    private string _relationshipEventsSummary;
    private bool _isSharingBorder;

	private Dictionary<EVENT_TYPES, bool> _eventBuffs;

	//Kingdom Threat
	private float _racePercentageModifier;
	private float _targetKingdomThreatLevel;

	internal int _theoreticalPower;
//	internal int _theoreticalDefense;
	private int _relativeStrength;

	internal int _usedSourceEffectivePower;
	internal int _usedSourceEffectiveDef;
	internal int _usedTargetEffectivePower;
	internal int _usedTargetEffectiveDef;

	internal SharedKingdomRelationship sharedRelationship;

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
	public Dictionary<RELATIONSHIP_MODIFIER, RelationshipModifier> relationshipModifiers {
		get { return this._relationshipModifiers; }
	}
    public string relationshipSummary {
        get { return this._relationshipSummary + this._relationshipEventsSummary; }
    }
    public int totalLike {
        get { return _like + GetTotalRelationshipModifiers() + forTestingLikeModifier; }
    }
    public int eventLikenessModifier {
        get { return _eventLikenessModifier; }
    }
    public bool isSharingBorder {
        get { return _isSharingBorder; }
    }
	public Dictionary<EVENT_TYPES, bool> eventBuffs {
		get { return this._eventBuffs; }
	}
	public int targetKingdomThreatLevel{
		get {
			return Math.Min (this._targetKingdom.warmongerValue, this._relativeStrength);
		}
	}
	public int relativeStrength{
		get { return this._relativeStrength; }
	}
//	public int targetKingdomThreat{
//		get { return Math.Min (this._targetKingdom.warmongerValue, this._relativeStrength);}
//	}
//	public int targetInvasionValue{
//		get {
//			if(!this._isAdjacent){
//				return 0;
//			}
//			return Math.Min (this._targetKingdom.warmongerValue, this._relativeWeakness);
//		}
//	}
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
		this._relationshipModifiers = new Dictionary<RELATIONSHIP_MODIFIER, RelationshipModifier> ();
        _like = 0;
        _eventLikenessModifier = 0;
        _relationshipSummary = string.Empty;
        _relationshipEventsSummary = string.Empty;
		this._theoreticalPower = 0;
		this._relativeStrength = 0;
		this.sharedRelationship = null;

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
    internal void UpdateLikeness() {
        this._relationshipSummary = string.Empty;
        int baseLoyalty = 0;
        int adjustment = 0;


        //Kingdom Type
        if (_sourceKingdom.kingdomTypeData.dictRelationshipKingdomType.ContainsKey(_targetKingdom.kingdomType)) {
            adjustment = _sourceKingdom.kingdomTypeData.dictRelationshipKingdomType[_targetKingdom.kingdomType];
            if(adjustment != 0) {
                baseLoyalty += adjustment;
                if (adjustment > 0) {
                    this._relationshipSummary += "+";
                }
                this._relationshipSummary += adjustment.ToString() + " Kingdom Type.\n";
            }
        }

        //Recent War
		if (this.sharedRelationship.isRecentWar) {
            adjustment = -30;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + " Recent War.\n";
        }

		//Race
		//if (this._sourceKingdom.race == this._targetKingdom.race) {
		//	adjustment = 30;
		//	baseLoyalty += adjustment;
		//	this._relationshipSummary += "+" + adjustment.ToString() + " Same Race.\n";
		//}else{
		if(this._sourceKingdom.race != RACE.UNDEAD){
			if(this._targetKingdom.race == RACE.UNDEAD){
				adjustment = -50;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString() + " Undead Race.\n";
			}else{
				if (this._sourceKingdom.race != this._targetKingdom.race) {
					adjustment = -30;
					baseLoyalty += adjustment;
					this._relationshipSummary += adjustment.ToString() + " Different Race.\n";
				}
			}
		}else{
			if (this._targetKingdom.race != RACE.UNDEAD) {
				adjustment = -30;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString() + " Living.\n";
			}
		}


        //Traits
        if (this._sourceKingdom.king.HasTrait(TRAIT.HOSTILE)) {
            if (this._targetKingdom.king.HasTrait(TRAIT.DIPLOMATIC)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Diplomatic.\n";
            }
        }

        if (this._sourceKingdom.king.HasTrait(TRAIT.DIPLOMATIC)) {
            if (this._targetKingdom.king.HasTrait(TRAIT.HOSTILE)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Hostile.\n";
            }
        }

        if (this._sourceKingdom.king.HasTrait(TRAIT.RUTHLESS)) {
            if (this._targetKingdom.king.HasTrait(TRAIT.BENEVOLENT)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Benevolent.\n";
            }
        }

        if (this._sourceKingdom.king.HasTrait(TRAIT.BENEVOLENT)) {
            if (this._targetKingdom.king.HasTrait(TRAIT.RUTHLESS)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Ruthless.\n";
            }
        }

        if (this._sourceKingdom.king.HasTrait(TRAIT.HONEST)) {
            if (this._targetKingdom.king.HasTrait(TRAIT.DECEITFUL)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Deceitful.\n";
            }

            if (this._targetKingdom.king.HasTrait(TRAIT.SCHEMING)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Scheming.\n";
            }
        }

        if (this._sourceKingdom.king.HasTrait(TRAIT.DEFENSIVE)) {
            if (this._targetKingdom.king.HasTrait(TRAIT.IMPERIALIST)) {
                adjustment = -20;
                baseLoyalty += adjustment;
                this._relationshipSummary += adjustment.ToString() + " Dislikes Imperialist.\n";
            }
        }

        ////Charisma Trait
        //if(this._targetKingdom.king.charisma == TRAIT.CHARISMATIC){
        //	adjustment = 15;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += "+" + adjustment.ToString() + " Charmed.\n";
        //}else if(this._targetKingdom.king.charisma == TRAIT.REPULSIVE){
        //	adjustment = -15;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += adjustment.ToString() + " Repulsed.\n";
        //}

        ////Military Trait
        //if(this._sourceKingdom.king.military == TRAIT.PACIFIST){
        //	if(this._targetKingdom.king.military != TRAIT.HOSTILE){
        //		adjustment = 30;
        //		baseLoyalty += adjustment;
        //		this._relationshipSummary += "+" + adjustment.ToString() + " Pacifist.\n";
        //	}else{
        //		adjustment = -30;
        //		baseLoyalty += adjustment;
        //		this._relationshipSummary += adjustment.ToString() + " Disapproved Hostility.\n";
        //	}
        //}else if(this._sourceKingdom.king.military == TRAIT.HOSTILE){
        //	adjustment = -30;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += adjustment.ToString() + " Hostile.\n";
        //}

        ////Intelligence Trait
        //if(this._sourceKingdom.king.intelligence == TRAIT.SMART && this._targetKingdom.king.intelligence == TRAIT.SMART){
        //	adjustment = 30;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += "+" + adjustment.ToString() + " Both Smart.\n";
        //}else if(this._sourceKingdom.king.intelligence == TRAIT.SMART && this._targetKingdom.king.intelligence == TRAIT.DUMB){
        //	adjustment = -30;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += adjustment.ToString() + " Dislikes Dumb.\n";
        //}

        ////Efficieny Trait
        //if(this._sourceKingdom.king.efficiency == TRAIT.EFFICIENT && this._targetKingdom.king.efficiency == TRAIT.EFFICIENT){
        //	adjustment = 30;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += "+" + adjustment.ToString() + " Both Efficient.\n";
        //}else if(this._sourceKingdom.king.efficiency == TRAIT.EFFICIENT && this._targetKingdom.king.efficiency == TRAIT.INEFFICIENT){
        //	adjustment = -30;
        //	baseLoyalty += adjustment;
        //	this._relationshipSummary += adjustment.ToString() + " Dislikes Inept.\n";
        //}


        //Kingdom Threat
        string summary = string.Empty;
		adjustment = GetKingdomThreatOpinionChangeBasedOnTrait (TRAIT.OPPORTUNIST, out summary);
		if (adjustment != 0) {
			baseLoyalty += adjustment;
			if (adjustment >= 0) {
				this._relationshipSummary += "+";
			}
			this._relationshipSummary += adjustment.ToString () + " " + summary + "\n";
		} else {
			int threat = this.targetKingdomThreatLevel;
			if(threat >= 100){
				adjustment = -50;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString () + " Fears Power\n";
			}else if(threat > 50 && threat < 100){
				adjustment = -30;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString () + " Fears Power\n";
			}else if(threat > 25 && threat <= 50){
				adjustment = -15;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString () + " Fears Power\n";
			}
		}

		adjustment = GetKingdomThreatOpinionChangeBasedOnTrait (TRAIT.IMPERIALIST, out summary);
		if(adjustment != 0){
			baseLoyalty += adjustment;
			if(adjustment >= 0){
				this._relationshipSummary += "+";
			}
			this._relationshipSummary += adjustment.ToString() + " " + summary + "\n";
		}

		adjustment = GetKingdomThreatOpinionChangeBasedOnTrait (TRAIT.BENEVOLENT, out summary);
		if(adjustment != 0){
			baseLoyalty += adjustment;
			if(adjustment >= 0){
				this._relationshipSummary += "+";
			}
			this._relationshipSummary += adjustment.ToString() + " " + summary + "\n";
		}

        this._like = 0;
        this.AdjustLikeness(baseLoyalty);
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
    internal void AdjustLikeness(int adjustment) {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
        this._like += adjustment;
        this._like = Mathf.Clamp(this._like, -100, 100);
        this.UpdateKingRelationshipStatus();
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
        UpdateLikeness();
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
	internal void AddRelationshipModifier(int modification, string reason, RELATIONSHIP_MODIFIER identifier, bool isDecaying, bool isExpiring){
		GameDate dateTimeToUse = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		if(identifier == RELATIONSHIP_MODIFIER.LEAVE_ALLIANCE){
			dateTimeToUse.SetDate (dateTimeToUse.month, 1, dateTimeToUse.year);
		}else if(identifier == RELATIONSHIP_MODIFIER.REBELLION){
			dateTimeToUse.SetDate (dateTimeToUse.month, 1, dateTimeToUse.year);
		}else if(identifier == RELATIONSHIP_MODIFIER.FLATTER){
			dateTimeToUse.SetDate (dateTimeToUse.month, 1, dateTimeToUse.year);
		}
		if (isDecaying) {
			AddDecayingRelationshipModifier (modification, reason, identifier, dateTimeToUse);
		} else {
			if(!isExpiring){
				AddNormalRelationshipModifier (modification, reason, identifier);
			}
		}
        UpdateKingRelationshipStatus();
	}
	private void AddNormalRelationshipModifier(int modification, string reason, RELATIONSHIP_MODIFIER identifier){
		if(this._relationshipModifiers.ContainsKey(identifier)){
			RelationshipModifier relationshipModifier = this._relationshipModifiers [identifier];
			relationshipModifier.SetModifier (modification);
		}else{
			RelationshipModifier relationshipModifier = new RelationshipModifier (modification, reason, identifier);
			this._relationshipModifiers.Add (identifier, relationshipModifier);
		}
	}
	private void AddDecayingRelationshipModifier(int modification, string reason, RELATIONSHIP_MODIFIER identifier, GameDate currentDate){
		RelationshipModifier relationshipModifier = null;
		if(this._relationshipModifiers.ContainsKey(identifier)){
			relationshipModifier = this._relationshipModifiers [identifier];
			relationshipModifier.SetModifier (modification);
		}
		if(relationshipModifier == null){
			relationshipModifier = new RelationshipModifier (modification, reason, identifier);
			this._relationshipModifiers.Add (identifier, relationshipModifier);
			if(identifier == RELATIONSHIP_MODIFIER.LEAVE_ALLIANCE){
				currentDate.AddMonths (1);
				SchedulingManager.Instance.AddEntry (currentDate, () => DecayRelationshipModifier(relationshipModifier, 5, DECAY_INTERVAL.MONTHLY, 1));
			}else if(identifier == RELATIONSHIP_MODIFIER.REBELLION){
				currentDate.AddMonths (1);
				SchedulingManager.Instance.AddEntry (currentDate, () => DecayRelationshipModifier(relationshipModifier, 5, DECAY_INTERVAL.MONTHLY, 1));
			}else if(identifier == RELATIONSHIP_MODIFIER.FLATTER){
				int decayAmount = 5;
				if(modification >= 0){
					decayAmount = -5;
				}
				currentDate.AddMonths (1);
				SchedulingManager.Instance.AddEntry (currentDate, () => DecayRelationshipModifier(relationshipModifier, decayAmount, DECAY_INTERVAL.MONTHLY, 1));
			}
		}
	}
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
	private void DecayRelationshipModifier(RelationshipModifier relationshipModifier, int amount, DECAY_INTERVAL decayInterval, int interval){
		if (this._relationshipModifiers.ContainsKey (relationshipModifier.identifier)) {
			relationshipModifier.AdjustModifier (amount);
			if (relationshipModifier.modifier != 0) {
				GameDate decayDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
				if (decayInterval == DECAY_INTERVAL.DAILY) {
					decayDate.AddDays (interval);
				} else if (decayInterval == DECAY_INTERVAL.MONTHLY) {
					decayDate.AddMonths (interval);
				} else if (decayInterval == DECAY_INTERVAL.YEARLY) {
					decayDate.AddYears (interval);
				}
				SchedulingManager.Instance.AddEntry (decayDate, () => DecayRelationshipModifier (relationshipModifier, amount, decayInterval, interval));
			} else {
				this._relationshipModifiers.Remove (relationshipModifier.identifier);
			}
		}
		UpdateKingRelationshipStatus();
	}
	internal void RemoveRelationshipModifier(RELATIONSHIP_MODIFIER relationshipMod){
		if(this._relationshipModifiers.ContainsKey(relationshipMod)){
			this._relationshipModifiers.Remove (relationshipMod);
			UpdateKingRelationshipStatus ();
		}
	}
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
	private int GetTotalRelationshipModifiers(){
		if(this._relationshipModifiers.Count > 0){
			int totalModifier = this._relationshipModifiers.Values.Sum (x => x.modifier);
			Debug.Log (this._sourceKingdom.name + " relationship modifier to " + this._targetKingdom.name + " : " + totalModifier.ToString());
			return totalModifier;
		}else{
			return 0;
		}
	}
    #endregion

    #region Relationship History
    internal void AddRelationshipHistory(History relHistory) {
        _relationshipHistory.Add(relHistory);
    }
    #endregion

	internal void ChangeAdjacency(bool state){
		this.sharedRelationship.SetAdjacency (state);
		if (state){
			this._sourceKingdom.AddAdjacentKingdom (this._targetKingdom);
			this._targetKingdom.AddAdjacentKingdom (this._sourceKingdom);
		}else{
			this._sourceKingdom.RemoveAdjacentKingdom (this._targetKingdom);
			this._targetKingdom.RemoveAdjacentKingdom (this._sourceKingdom);
		}
	}

	internal void ChangeWarStatus(bool state, Warfare warfare){
		this.sharedRelationship.SetWarStatus (state, warfare);
	}
	internal void ChangeBattle(Battle battle){
		this.sharedRelationship.SetBattle (battle);
	}
	internal void ChangeDiscovery(bool state){
		this.sharedRelationship.SetDiscovery (state);
	}
	internal void ChangeRecentWar(bool state){
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
			this.sharedRelationship.SetRecentWar (state);
		}
	}

	private void SetRaceThreatModifier(){
		if(this._sourceKingdom.race != this._targetKingdom.race){
			this._racePercentageModifier = 1.15f;
		}else{
			this._racePercentageModifier = 1f;
		}
	}
	internal void UpdateThreatLevel(){
		KingdomRelationship rk = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		UpdateTheoreticalPower ();
		rk.UpdateTheoreticalPower ();

		this._relativeStrength = (int)((((float)rk._theoreticalPower / (float)this._theoreticalPower) * 100f) - 100f);
		this._relativeStrength = Mathf.Clamp (this._relativeStrength, -100, 100);

		float threat = this.targetKingdomThreatLevel;
		if(this.sharedRelationship.isAdjacent){
			if(threat >= 100){
				this._sourceKingdom.has100OrAboveThreat = true;
			}
			if(threat > 50f){
				if(this._sourceKingdom.highestThreatAdjacentKingdomAbove50 == null){
					this._sourceKingdom.highestThreatAdjacentKingdomAbove50 = this._targetKingdom;
					this._sourceKingdom.highestThreatAdjacentKingdomAbove50Value = threat;
				}else{
					if(threat > this._sourceKingdom.highestThreatAdjacentKingdomAbove50Value){
						this._sourceKingdom.highestThreatAdjacentKingdomAbove50 = this._targetKingdom;
						this._sourceKingdom.highestThreatAdjacentKingdomAbove50Value = threat;
					}
				}
			}
			if(this._relativeStrength > 0){
				if(this._sourceKingdom.highestRelativeStrengthAdjacentKingdom == null){
					this._sourceKingdom.highestRelativeStrengthAdjacentKingdom = this._targetKingdom;
					this._sourceKingdom.highestRelativeStrengthAdjacentKingdomValue = this._relativeStrength;
				}else{
					if(this._relativeStrength > this._sourceKingdom.highestRelativeStrengthAdjacentKingdomValue){
						this._sourceKingdom.highestRelativeStrengthAdjacentKingdom = this._targetKingdom;
						this._sourceKingdom.highestRelativeStrengthAdjacentKingdomValue = this._relativeStrength;
					}
				}
			}
		}
    }
	internal void UpdateTheoreticalPower(){
		int posAllianceSoldiers = GetAdjacentPosAllianceSoldiers ();
		int otherAdjacentEnemiesPower = GetOtherAdjacentEnemiesPower();

		this._theoreticalPower = this._sourceKingdom.soldiersCount + posAllianceSoldiers - otherAdjacentEnemiesPower;
	}
	private int GetAdjacentPosAllianceSoldiers(){
		int posAlliancePower = 0;
		if(this._sourceKingdom.alliancePool != null){
			for (int i = 0; i < this._sourceKingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this._sourceKingdom.alliancePool.kingdomsInvolved[i];
				if(this._sourceKingdom.id != kingdomInAlliance.id && this._targetKingdom.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this._sourceKingdom);
					KingdomRelationship relationshipToEnemy = kingdomInAlliance.GetRelationshipWithKingdom(this._targetKingdom);
					if(relationship.totalLike >= 0 && relationshipToEnemy.sharedRelationship.isAdjacent && !relationshipToEnemy.sharedRelationship.isAtWar){
						posAlliancePower += (int)((float)kingdomInAlliance.soldiersCount * GetOpinionPercentage(relationship.totalLike));
					}
				}
			}
		}
		return posAlliancePower;
	}

	// This function returns the sum of all soldiers of all enemy kingdoms except the one in this instance
	// who are not allied with the current source kingdom
	private int GetOtherAdjacentEnemiesPower(){
		int otherEnemiesPower = 0;
		foreach (KingdomRelationship kr in this._sourceKingdom.relationships.Values) {
			if(this._sourceKingdom.id != kr.targetKingdom.id && this._targetKingdom.id != kr.targetKingdom.id){
				if(kr.sharedRelationship.isAtWar && kr.sharedRelationship.isAdjacent){
					otherEnemiesPower += kr.targetKingdom.soldiersCount;
				}
			}
		}
		return otherEnemiesPower;
	}

	private float GetOpinionPercentage(int opinion){
		if(opinion >= 0 && opinion < 35){
			return 0.25f;
		}else if(opinion >= 35 && opinion < 50){
			return 0.5f;
		}else{
			return 1f;
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
	private int GetKingdomThreatOpinionChangeBasedOnTrait(TRAIT trait, out string summary){
		//Kingdom Threat
		summary = string.Empty;
		if (!this._sourceKingdom.king.otherTraits.Contains (trait)) {
			return 0;
		}
		float threat = this.targetKingdomThreatLevel;
		if(trait == TRAIT.OPPORTUNIST){
			if(threat >= 100){
				summary = "Respects Power";
				return 50;
			}else if(threat > 50 && threat < 100){
				summary = "Respects Power";
				return 30;
			}else if(threat > 25 && threat <= 50){
				summary = "Respects Power";
				return 15;
			}else if(threat >= -50 && threat < -25){
				summary = "Scorns Weakness";
				return -15;
			}else if(threat > -100 && threat < -50){
				summary = "Scorns Weakness";
				return -30;
			}else if(threat <= -100){
				summary = "Scorns Weakness";
				return -50;
			}
		}else if(trait == TRAIT.IMPERIALIST){
			if(threat >= -50 && threat < -25){
				summary = "Preys on the Weak";
				return -15;
			}else if(threat > -100 && threat < -50){
				summary = "Preys on the Weak";
				return -30;
			}else if(threat <= -100){
				summary = "Preys on the Weak";
				return -50;
			}
		}else if(trait == TRAIT.BENEVOLENT){
			if(threat >= -50 && threat < -25){
				summary = "Protects the Weak";
				return 15;
			}else if(threat > -100 && threat < -50){
				summary = "Protects the Weak";
				return 30;
			}else if(threat <= -100){
				summary = "Protects the Weak";
				return 50;
			}
		}
		return 0;
	}

	internal void ChangeCantAlly(bool state){
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
			this.sharedRelationship.SetCantAlly (state);
			if(state){
				GameDate changeDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
				changeDate.AddYears (1);
				SchedulingManager.Instance.AddEntry (changeDate, () => ChangeCantAlly (false));
			}
		}
	}

    internal List<Kingdom> GetAlliesTargetKingdomIsAtWarWith() {
        List<Kingdom> alliesAtWarWith = new List<Kingdom>();
        for (int i = 0; i < targetKingdom.warfareInfo.Values.Count; i++) {
            WarfareInfo currWarfare = targetKingdom.warfareInfo.Values.ElementAt(i);
            WAR_SIDE sideOfTargetKingdom = currWarfare.side;
            WAR_SIDE opposingSide = WAR_SIDE.A;
            if(sideOfTargetKingdom == WAR_SIDE.A) {
                opposingSide = WAR_SIDE.B;
            }
            List<Kingdom> enemyKingdoms = currWarfare.warfare.GetListFromSide(opposingSide);
            for (int j = 0; j < enemyKingdoms.Count; j++) {
                Kingdom currEnemy = enemyKingdoms[j];
                if (sourceKingdom.alliancePool.kingdomsInvolved.Contains(currEnemy)) {
                    //targetKingdom is at war with sourceKingdom ally(currEnemy)
                    if (!alliesAtWarWith.Contains(currEnemy)) {
                        alliesAtWarWith.Add(currEnemy);
                    }
                }
            }


        }
        return alliesAtWarWith;
    }
    internal float GetTheoreticalPowerAdvantageOverTarget() {
        KingdomRelationship otherRelationship = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);
        return Mathf.Max(0, (otherRelationship._theoreticalPower / this._theoreticalPower) - 100f);
    }
}
