using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RelationshipKings {
//	public int id;
	public Citizen sourceKing;
	public Citizen king;
//	public DECISION previousDecision;
	public int like;
	public RELATIONSHIP_STATUS lordRelationship;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isFirstEncounter;
//	public bool isAdjacent;
//	public bool isAtWar;
	public int daysAtWar;
	public List<History> relationshipHistory;

    private const int CHANCE_TO_CANCEL_TRADE_ROUTE = 10;

	public RelationshipKings(Citizen sourceKing, Citizen king, int like){
//		this.id = id;
		this.sourceKing = sourceKing;
		this.king = king;
//		this.previousDecision = previousDecision;
		this.like = like;
		this.isFirstEncounter = true;
		this.lordRelationship = RELATIONSHIP_STATUS.NEUTRAL;
		this.relationshipHistory = new List<History>();
//		this.isAdjacent = false;
//		this.isAtWar = false;
	}

	internal void UpdateKingRelationshipStatus(){
		if(this.like <= -81){
			this.lordRelationship = RELATIONSHIP_STATUS.RIVAL;
		}else if(this.like >= -80 && this.like <= -41){
			this.lordRelationship = RELATIONSHIP_STATUS.ENEMY;
		}else if(this.like >= -40 && this.like <= -21){
			this.lordRelationship = RELATIONSHIP_STATUS.COLD;
		}else if(this.like >= -20 && this.like <= 20){
			this.lordRelationship = RELATIONSHIP_STATUS.NEUTRAL;
		}else if(this.like >= 21 && this.like <= 40){
			this.lordRelationship = RELATIONSHIP_STATUS.WARM;
		}else if(this.like >= 41 && this.like <= 80){
			this.lordRelationship = RELATIONSHIP_STATUS.FRIEND;
		}else{
			this.lordRelationship = RELATIONSHIP_STATUS.ALLY;
		}
	}

	// This will change the view of sourceKing towards King
	// Parameters:
	//		adjustment - relationship value change
	//		gameEvent - event that caused the relationship change
	//		isDiscovery - flag to determine whether the relationship change was because the sourceKing found out the gameEvent
	internal void AdjustLikeness(int adjustment, GameEvent gameEventTrigger, bool isDiscovery = false){
        //		if (adjustment < 0) {
        //			//Deteriorating
        //			if (this.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
        //				adjustment *= 0.75f;
        //			}
        //			if (this.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
        //				adjustment *= 1.25f;
        //			}
        //
        //		} else {
        //			//Increasing
        //			if (this.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
        //				adjustment *= 1.25f;
        //			}
        //			if (this.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
        //				adjustment *= 0.75f;
        //			}
        //
        //		}

        RELATIONSHIP_STATUS previousStatus = this.lordRelationship;
		this.like += adjustment;
		this.like = Mathf.Clamp(this.like, -100, 100);

//		if(this.like < -100){
//			this.like = -100;
//		}
//		else if(this.like > 100){
//			this.like = 100;
//		}
		this.UpdateKingRelationshipStatus ();
        if (adjustment < 0) { //Relationship deteriorated
            sourceKing.DeteriorateRelationship(this, gameEventTrigger, isDiscovery);
            this.CheckForEmbargo(previousStatus, gameEventTrigger);
        } else { //Relationship improved
            sourceKing.ImproveRelationship(this);
            this.CheckForDisembargo(previousStatus);
        }
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
            } else {
                int chance = Random.Range(0, 100);
                if (chance < CHANCE_TO_CANCEL_TRADE_ROUTE) {
                    RemoveTradeRoutes();
                }
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
        Debug.LogError(this.sourceKing.city.kingdom.name + " put " + this.king.city.kingdom.name + " in it's embargo list, beacuase of " + gameEventReasonForEmbargo.eventType.ToString());
    }

    /*
     * Remove all trade routes with targetKingdom
     * */
    protected void RemoveTradeRoutes() {
        this.sourceKing.city.kingdom.RemoveAllTradeRoutesWithOtherKingdom(this.king.city.kingdom);
        this.king.city.kingdom.RemoveAllTradeRoutesWithOtherKingdom(this.sourceKing.city.kingdom);
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

    #region For Testing Functions
    /*
     * Instantly change the like rating
     * of sourceKing towards targetKing.
     * */
    internal void ChangeSourceKingLikeness(int newLikeness){
		this.like = newLikeness;
		this.UpdateKingRelationshipStatus();
	}

    /*
    * Instantly change the like rating
    * of targetKing towards sourceking.
    * */
    internal void ChangeTargetKingLikeness(int newLikeness){
		RelationshipKings rel = this.king.GetRelationshipWithCitizen(this.sourceKing);
		rel.like = newLikeness;
		rel.UpdateKingRelationshipStatus();
	}
    #endregion
}
