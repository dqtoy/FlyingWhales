using UnityEngine;
using System.Collections;

public class Relationship<T> {

	public T objectInRelationship;
//	public DECISION previousDecision;
	public int like;
	public RELATIONSHIP_STATUS relationshipStatus;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isFirstEncounter;
	public bool isAdjacent;
	public bool isAtWar;
//	public int daysAtWar;

	public Relationship(T objectInRelationship){
		this.objectInRelationship = objectInRelationship;
		this.like = 0;
		this.UpdateRelationshipStatusBasedOnLike();
		this.isFirstEncounter = true;
		this.isAtWar = false;
		this.isAdjacent = false;
	}

	protected void UpdateRelationshipStatusBasedOnLike(){
		if (like <= 100 && like >= 81) {
			this.relationshipStatus = RELATIONSHIP_STATUS.ALLY;
		} else if(like <= 80 && like >= 41) {
			this.relationshipStatus = RELATIONSHIP_STATUS.FRIEND;
		} else if(like <= 40 && like >= 21) {
			this.relationshipStatus = RELATIONSHIP_STATUS.WARM;
		} else if(like <= 20 && like >= -20) {
			this.relationshipStatus = RELATIONSHIP_STATUS.NEUTRAL;
		} else if(like <= -21 && like >= -40) {
			this.relationshipStatus = RELATIONSHIP_STATUS.COLD;
		} else if(like <= -41 && like >= -80) {
			this.relationshipStatus = RELATIONSHIP_STATUS.ENEMY;
		} else if(like <= -80 && like >= -100) {
			this.relationshipStatus = RELATIONSHIP_STATUS.RIVAL;
		}
	}

	internal void AdjustLikeness(int adjustment){
		this.like += adjustment;
		this.UpdateRelationshipStatusBasedOnLike();
	}

}
