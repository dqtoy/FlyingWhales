using UnityEngine;
using System.Collections;

[System.Serializable]
public class RelationshipKingdom {

	public Kingdom sourceKingdom;
	public Kingdom objectInRelationship;
//	public DECISION previousDecision;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isAdjacent;
	public bool isAtWar;
	public KingdomWar kingdomWar;
	public MONTH monthToMoveOnAfterRejection;
	public War war;
	public InvasionPlan invasionPlan;
//	public int daysAtWar;

	public RelationshipKingdom(Kingdom sourceKingdom, Kingdom objectInRelationship){
		this.sourceKingdom = sourceKingdom;
		this.objectInRelationship = objectInRelationship;
		this.isAtWar = false;
		this.isAdjacent = false;
		this.kingdomWar = new KingdomWar (objectInRelationship);
		this.monthToMoveOnAfterRejection = MONTH.NONE;
	}

	internal void AdjustExhaustion(int amount){
		if(this.isAtWar){
			this.kingdomWar.AdjustExhaustion (amount);
		}

	}

	internal void MoveOnAfterRejection(){
		if ((MONTH)GameManager.Instance.month == monthToMoveOnAfterRejection) {
			EventManager.Instance.onWeekEnd.RemoveListener(MoveOnAfterRejection);
			this.monthToMoveOnAfterRejection = MONTH.NONE;
		}
	}

	internal void CreateInvasionPlan(){
		this.invasionPlan = new InvasionPlan (this.sourceKingdom, this.objectInRelationship, this.war);
	}
}
