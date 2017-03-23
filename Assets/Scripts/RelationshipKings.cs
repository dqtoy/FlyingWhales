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



	internal RELATIONSHIP_STATUS GetLordRelationship(int likeness){
		if(likeness <= -81){
			return RELATIONSHIP_STATUS.RIVAL;
		}else if(likeness >= -80 && likeness <= -41){
			return RELATIONSHIP_STATUS.ENEMY;
		}else if(likeness >= -40 && likeness <= -21){
			return RELATIONSHIP_STATUS.COLD;
		}else if(likeness >= -20 && likeness <= 20){
			return RELATIONSHIP_STATUS.NEUTRAL;
		}else if(likeness >= 21 && likeness <= 40){
			return RELATIONSHIP_STATUS.WARM;
		}else if(likeness >= 41 && likeness <= 80){
			return RELATIONSHIP_STATUS.FRIEND;
		}else{
			return RELATIONSHIP_STATUS.ALLY;
		}
	}
}
