using UnityEngine;
using System.Collections;

[System.Serializable]
public class RelationshipKingdom {

	public Kingdom objectInRelationship;
//	public DECISION previousDecision;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isAdjacent;
	public bool isAtWar;
	public KingdomWar kingdomWar;
//	public int daysAtWar;

	public RelationshipKingdom(Kingdom objectInRelationship){
		this.objectInRelationship = objectInRelationship;
		this.isAtWar = false;
		this.isAdjacent = false;
		this.kingdomWar = new KingdomWar (objectInRelationship);
	}

	internal void AdjustExhaustion(int amount){
		if(this.isAtWar){
			this.kingdomWar.AdjustExhaustion (amount);
		}
	}

}
