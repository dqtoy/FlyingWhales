using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class RelationshipKings {
//	public int id;
	public Citizen sourceKing;
	public Citizen king;
//	public DECISION previousDecision;
	private int _like;
	public RELATIONSHIP_STATUS lordRelationship;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isFirstEncounter;
//	public bool isAdjacent;
//	public bool isAtWar;
	public int daysAtWar;
	public List<History> relationshipHistory;
	private List<ExpirableModifier> _eventModifiers;
//    private Dictionary<EVENT_TYPES, List<ExpirableModifier>> _eventModifiers;
    private int _eventLikenessModifier;
    private int _likeFromMutualRelationships;
	private string _relationshipSummary;
	internal string _relationshipEventsSummary;

	private bool _isInitial;

    #region getters/setters
    public int totalLike {
		get { return this._like + this._eventModifiers.Sum(x => x.modifier) + this.likeFromMutualRelationships; }
    }
	public string relationshipSummary {
		get { return this._relationshipSummary + this._relationshipEventsSummary; }
	}
    public int baseLike {
        get { return this._like; }
    }
    public List<ExpirableModifier> eventModifiers {
        get { return this._eventModifiers; }
    }
    public int eventLikenessModifier {
        get { return this._eventLikenessModifier; }
    }
    public int likeFromMutualRelationships {
        get { return this._likeFromMutualRelationships; }
    }
    #endregion

    public RelationshipKings(Citizen sourceKing, Citizen king, int like){
//		this.id = id;
		this.sourceKing = sourceKing;
		this.king = king;
//		this.previousDecision = previousDecision;
		this._like = like;
		this.isFirstEncounter = true;
		this.lordRelationship = RELATIONSHIP_STATUS.NEUTRAL;
		this.relationshipHistory = new List<History>();
        this._eventModifiers = new List<ExpirableModifier>();
        this._eventLikenessModifier = 0;
        this._likeFromMutualRelationships = 0;
		this._isInitial = true;
        //		this.isAdjacent = false;
        //		this.isAtWar = false;
		UpdateLikeness(null);
        Messenger.AddListener("OnDayEnd", CheckEventModifiers);
    }

	internal void UpdateKingRelationshipStatus(){
        RELATIONSHIP_STATUS previousStatus = this.lordRelationship;
        if (this.totalLike <= -81){
			this.lordRelationship = RELATIONSHIP_STATUS.RIVAL;
		}else if(this.totalLike >= -80 && this.totalLike <= -41){
			this.lordRelationship = RELATIONSHIP_STATUS.ENEMY;
		}else if(this.totalLike >= -40 && this.totalLike <= -21){
			this.lordRelationship = RELATIONSHIP_STATUS.COLD;
		}else if(this.totalLike >= -20 && this.totalLike <= 20){
			this.lordRelationship = RELATIONSHIP_STATUS.NEUTRAL;
		}else if(this.totalLike >= 21 && this.totalLike <= 40){
			this.lordRelationship = RELATIONSHIP_STATUS.WARM;
		}else if(this.totalLike >= 41 && this.totalLike <= 80){
			this.lordRelationship = RELATIONSHIP_STATUS.FRIEND;
		}else{
			this.lordRelationship = RELATIONSHIP_STATUS.ALLY;
		}
        if(previousStatus != this.lordRelationship) {
            //relationship status changed
            this.sourceKing.UpdateMutualRelationships();
        }
	}

//    internal int GetLikenessModifiersFromEvent(EVENT_TYPES eventType) {
//        int likeModifierFromEvent = 0;
//        if (this._eventModifiers.ContainsKey(eventType)) {
//            List<ExpirableModifier> modifiers = this._eventModifiers[eventType];
//            for (int i = 0; i < modifiers.Count; i++) {
//                likeModifierFromEvent += modifiers[i].modifier;
//            }
//        }
//        return likeModifierFromEvent;
//    }

    internal void ResetMutualRelationshipModifier() {
        this._likeFromMutualRelationships = 0;
    }

    internal void AddMutualRelationshipModifier(int amount) {
        this._likeFromMutualRelationships += amount;
    }

    internal void ChangeRelationshipStatus(RELATIONSHIP_STATUS newStatus, GameEvent gameEventTrigger = null) {
        if(newStatus == RELATIONSHIP_STATUS.ALLY) {
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
        if(this.lordRelationship == RELATIONSHIP_STATUS.ENEMY || this.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
            Embargo(gameEventTrigger);
        }
    }

    internal void SetLikeness(int likeness) {
        this._like = likeness;
        UpdateKingRelationshipStatus();
    }

	// This will change the view of sourceKing towards King
	// Parameters:
	//		adjustment - relationship value change
	//		gameEvent - event that caused the relationship change
	//		isDiscovery - flag to determine whether the relationship change was because the sourceKing found out the gameEvent
	internal void AdjustLikeness(int adjustment, GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false){
        RELATIONSHIP_STATUS previousStatus = this.lordRelationship;
		this._like += adjustment;
		this._like = Mathf.Clamp(this.totalLike, -100, 100);
		this.UpdateKingRelationshipStatus ();

		if(!this._isInitial){
			if (this.totalLike < 0) { //Relationship deteriorated
				sourceKing.DeteriorateRelationship(this, gameEventTrigger, isDiscovery, assassinationReasons);
				this.CheckForEmbargo(previousStatus, gameEventTrigger);
			} else { //Relationship improved
				sourceKing.ImproveRelationship(this);
				this.CheckForDisembargo(previousStatus);
			}
		}else{
			this._isInitial = false;
		}
       
	}

	internal void UpdateLikeness(GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false){
		this._relationshipSummary = string.Empty;
		int baseLoyalty = 0;
		int adjustment = 0;

		Kingdom sourceKingdom = this.sourceKing.city.kingdom;
		Kingdom targetKingdom = this.king.city.kingdom;

		RelationshipKingdom relationshipKingdom = sourceKingdom.GetRelationshipWithOtherKingdom (targetKingdom);

		List<CHARACTER_VALUE> sourceKingValues = this.sourceKing.importantCharacterValues.Select(x => x.Key).ToList();
		List<CHARACTER_VALUE> targetKingValues = this.king.importantCharacterValues.Select(x => x.Key).ToList();

		List<CHARACTER_VALUE> valuesInCommon = sourceKingValues.Intersect(targetKingValues).ToList();

		//Kingdom Type
		if(sourceKingdom.kingdomTypeData.dictRelationshipKingdomType.ContainsKey(targetKingdom.kingdomType)){
			adjustment = sourceKingdom.kingdomTypeData.dictRelationshipKingdomType[targetKingdom.kingdomType];
			baseLoyalty += adjustment;
			if(adjustment >= 0){
				this._relationshipSummary += "+";
			}
			this._relationshipSummary += adjustment.ToString() + "   kingdom type.\n";
		}


		//At War
		if(relationshipKingdom != null){
			if(relationshipKingdom.isAtWar){
				adjustment = -30;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString() + "   at war.\n";
			}
		}

		//Race
		if(sourceKingdom.race != targetKingdom.race && !sourceKingValues.Contains(CHARACTER_VALUE.EQUALITY)){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + "   different race.\n";
		}

		//Sharing Border
		if(relationshipKingdom.isSharingBorder){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + "   shared borders.\n";
		}

		//Values
		if (valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).Count() == 1) {
			adjustment = 0;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + "   shared values.\n";
		} else if (valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).Count() == 2) {
			adjustment = 15;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + "   shared values.\n";
		} else if(valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).Count() >= 3) {
			adjustment = 30;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + "   shared values.\n";
		} else{
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
		this.AdjustLikeness (baseLoyalty, gameEventTrigger, assassinationReasons, isDiscovery);
	}

    /*
     * This function checks if the targetKing is to be embargoed,
     * if not, check if the source king will trigger to cancel trade routes
     * between himself and target king.
     * */
    protected void CheckForEmbargo(RELATIONSHIP_STATUS previousRelationshipStatus, GameEvent gameEventTrigger) {
        if (previousRelationshipStatus != this.lordRelationship) { // Check if the relationship between the 2 kings changed in status
            //Check if the source kings relationship with king has change to enemy or rival, if so, put king's kingdom in source king's embargo list
            if (this.lordRelationship == RELATIONSHIP_STATUS.ENEMY || this.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
                Embargo(gameEventTrigger);
            }
        }
    }

    /*
     * Put targetKing's kingdom in sourceKing's kingdom embargo list.
     * TODO: Add embargo reason from gameEventReasonForEmbargo when adding
     * kingdom to embargo list.
     * */
    protected void Embargo(GameEvent gameEventReasonForEmbargo) {
        this.sourceKing.city.kingdom.AddKingdomToEmbargoList(this.king.city.kingdom);
        //Debug.LogError(this.sourceKing.city.kingdom.name + " put " + this.king.city.kingdom.name + " in it's embargo list, beacuase of " + gameEventReasonForEmbargo.eventType.ToString());
    }

    /*
     * This function checks if the targetKing is to be disembargoed,
     * requirement/s for disembargo:
     * - Relationship should go from ENEMY to COLD.
     * */
    protected void CheckForDisembargo(RELATIONSHIP_STATUS previousRelationshipStatus) {
        if (previousRelationshipStatus != this.lordRelationship) { // Check if the relationship between the 2 kings changed in status
            //if the relationship changed from enemy to cold, disembargo the targetKing
            if (previousRelationshipStatus == RELATIONSHIP_STATUS.ENEMY && this.lordRelationship == RELATIONSHIP_STATUS.COLD) {
                Disembargo();
            }
        }
    }

    /*
     * Put targetKing's kingdom in sourceKing's kingdom embargo list
     * */
    protected void Disembargo() {
        this.sourceKing.city.kingdom.RemoveKingdomFromEmbargoList(this.king.city.kingdom);
        Debug.LogError(this.sourceKing.city.kingdom.name + " removed " + this.king.city.kingdom.name + " from it's embargo list!");
    }

    internal void ChangeSourceKing(Citizen newSourceKing) {
        this.sourceKing = newSourceKing;
    }

    internal void ChangeTargetKing(Citizen newTargetKing) {
        this.king = newTargetKing;
        ResetEventModifiers();
    }

    internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
//        DateTime dateTimeToUse = new DateTime(GameManager.Instance.year, GameManager.Instance.month, GameManager.Instance.days);
//        if(gameEventTrigger.eventType == EVENT_TYPES.KINGDOM_WAR) {
//            dateTimeToUse.AddYears(1);
//        }
		RELATIONSHIP_STATUS previousStatus = this.lordRelationship;

		GameDate dateTimeToUse = new GameDate();
		if(gameEventTrigger.eventType == EVENT_TYPES.KINGDOM_WAR) {
			dateTimeToUse.AddYears(1);
		}else{
			dateTimeToUse.AddMonths(3);
		}
		this._eventModifiers.Add (new ExpirableModifier (gameEventTrigger, summary, dateTimeToUse, modification));
//        if (this._eventModifiers.ContainsKey(gameEventTrigger.eventType)) {
//            this._eventModifiers[gameEventTrigger.eventType].Add(new ExpirableModifier(gameEventTrigger, summary, dateTimeToUse, modification));
//        } else {
//            this._eventModifiers.Add(gameEventTrigger.eventType, new List<ExpirableModifier>() {
//                { new ExpirableModifier(gameEventTrigger, summary, dateTimeToUse, modification) }
//            });
//        }
        
//        this._eventLikenessModifier += modification;
		if(modification < 0){
            sourceKing.DeteriorateRelationship(this, gameEventTrigger, isDiscovery, assassinationReasons);
			this.CheckForEmbargo(previousStatus, gameEventTrigger);

			if(gameEventTrigger != null){
				if(gameEventTrigger.eventType != EVENT_TYPES.KINGDOM_WAR) {
					this.sourceKing.WarTrigger(this, gameEventTrigger, this.sourceKing.city.kingdom.kingdomTypeData, gameEventTrigger.warTrigger);
				}
			}
            
		}else{
			sourceKing.ImproveRelationship(this);
			this.CheckForDisembargo(previousStatus);
		}
        UpdateKingRelationshipStatus();
    }

    internal void CheckEventModifiers() {
		for (int i = 0; i < this._eventModifiers.Count; i++) {
			ExpirableModifier expMod = this._eventModifiers [i];
			if(expMod.dueDate.day == GameManager.Instance.days && expMod.dueDate.month == GameManager.Instance.month && expMod.dueDate.year == GameManager.Instance.year){
				RemoveEventModifierAt (i);
				i--;
			}

		}
//        if (this._eventModifiers.ContainsKey(EVENT_TYPES.KINGDOM_WAR)) {
//            List<ExpirableModifier> expirableModifiers = this._eventModifiers[EVENT_TYPES.KINGDOM_WAR];
//            List<ExpirableModifier> modifiersToRemove = new List<ExpirableModifier>();
//            for (int i = 0; i < expirableModifiers.Count; i++) {
//                ExpirableModifier currModifier = expirableModifiers[i];
//                GameDate dueDate = currModifier.dueDate;
//                int modification = currModifier.modifier;
//                if (dueDate.day == GameManager.Instance.days && dueDate.month == GameManager.Instance.month
//                    && dueDate.year == GameManager.Instance.year) {
//                    modification += 5;
//                    dueDate.AddYears(1);
//                    if (modification >= 0) {
//                        modifiersToRemove.Add(currModifier);
//                    } else {
//                        currModifier.SetModifier(modification);
//                        currModifier.SetDueDate(dueDate);
//                    }
//                }
//            }
//            for (int i = 0; i < modifiersToRemove.Count; i++) {
//                expirableModifiers.Remove(modifiersToRemove[i]);
//            }
//            UpdateEventModifiers();
//        }
    }
	private void RemoveEventModifierAt(int index){
		this._eventModifiers.RemoveAt (index);
		UpdateKingRelationshipStatus();
	}
//    private void UpdateEventModifiers() {
//        this._eventLikenessModifier = 0;
//        for (int i = 0; i < this._eventModifiers.Count; i++) {
//            EVENT_TYPES key = this._eventModifiers.Keys.ElementAt(i);
//            List<ExpirableModifier> modifiers = this._eventModifiers[key];
//            for (int j = 0; j < modifiers.Count; j++) {
//                this._eventLikenessModifier += modifiers[j].modifier;
//            }
//        }
//    }

    private void ResetEventModifiers() {
        this._eventLikenessModifier = 0;
        this._eventModifiers.Clear();
        UpdateKingRelationshipStatus();
    }
    #region For Testing Functions
    /*
     * Instantly change the like rating
     * of sourceKing towards targetKing.
     * */
    internal void ChangeSourceKingLikeness(int newLikeness){
		this._like = newLikeness;
		this.UpdateKingRelationshipStatus();
	}

    /*
    * Instantly change the like rating
    * of targetKing towards sourceking.
    * */
    internal void ChangeTargetKingLikeness(int newLikeness){
		RelationshipKings rel = this.king.GetRelationshipWithCitizen(this.sourceKing);
		rel._like = newLikeness;
		rel.UpdateKingRelationshipStatus();
	}
    #endregion
}
