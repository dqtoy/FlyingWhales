using UnityEngine;
using System.Collections;

[System.Serializable]
public class RelationshipKings {
//	public int id;
	public Citizen king;
//	public DECISION previousDecision;
	public int like;
	public RELATIONSHIP_STATUS lordRelationship;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isFirstEncounter;
	public bool isAdjacent;
	public bool isAtWar;
	public int daysAtWar;

	public RelationshipKings(Citizen king, int like){
//		this.id = id;
		this.king = king;
//		this.previousDecision = previousDecision;
		this.like = like;
		this.isFirstEncounter = true;
		this.lordRelationship = RELATIONSHIP_STATUS.NEUTRAL;
		this.isAdjacent = false;
		this.isAtWar = false;
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

	internal void AdjustLikeness(int adjustment){
		this.like += adjustment;
		this.UpdateKingRelationshipStatus ();
	}
}
