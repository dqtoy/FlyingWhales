using UnityEngine;
using System.Collections;

public class War : GameEvent {

	internal Kingdom kingdom1;
	internal Kingdom kingdom2;

	internal RelationshipKingdom kingdom1Rel;
	internal RelationshipKingdom kingdom2Rel;

	internal InvasionPlan invasionPlanThatStartedWar;

	public War(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2, InvasionPlan invasionPlanThatStartedWar) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.KINGDOM_WAR;
		this.description = "War between " + kingdom1.name + " and " + kingdom2.name + ".";
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.kingdom1Rel = kingdom1.GetRelationshipWithOtherKingdom(kingdom2);
		this.kingdom2Rel = kingdom2.GetRelationshipWithOtherKingdom(kingdom1);
		this.invasionPlanThatStartedWar = invasionPlanThatStartedWar;

		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void DoneEvent(){
		this.isActive = false;
	}
}
