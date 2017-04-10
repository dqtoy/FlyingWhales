using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RelationshipKings {
//	public int id;
	public Citizen sourceKing;
	public Citizen king;
//	public DECISION previousDecision;
	public float like;
	public RELATIONSHIP_STATUS lordRelationship;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isFirstEncounter;
//	public bool isAdjacent;
//	public bool isAtWar;
	public int daysAtWar;
	public List<History> relationshipHistory;

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

	internal void AdjustLikeness(float adjustment, EVENT_TYPES reason = EVENT_TYPES.ALL, bool isDiscovery = false){
		if (adjustment < 0) {
			//Deteriorating
			if (this.sourceKing.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
				adjustment *= 0.75f;
			}
			if (this.sourceKing.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
				adjustment *= 1.25f;
			}

		} else {
			//Increasing
			if (this.sourceKing.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
				adjustment *= 1.25f;
			}
			if (this.sourceKing.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
				adjustment *= 0.75f;
			}

		}
		this.like += adjustment;
		if(this.like < -100){
			this.like = -100;
		}
		else if(this.like > 100){
			this.like = 100;
		}
		this.UpdateKingRelationshipStatus ();
		if (adjustment < 0) {
			sourceKing.DeteriorateRelationship (this, reason, isDiscovery);
		}else{
			sourceKing.ImproveRelationship (this);
		}
	}
}
