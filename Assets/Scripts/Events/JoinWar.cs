using UnityEngine;
using System.Collections;

public class JoinWar : GameEvent {

	public Citizen target;
	public Envoy envoy;
	public Kingdom targetKingdom;

	public JoinWar(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.JOIN_WAR_REQUEST;
		this.description = startedBy.name + " is looking for a suitable wife as the vessel of his heir";
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;


		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		} else {
			if (EventManager.Instance.GetEventsOfTypePerKingdom (this.target.city.kingdom, EVENT_TYPES.INVASION_PLAN).Count > 0) {
				//fail
			} else {
				int successRate = 15;
				RELATIONSHIP_STATUS relationshipWithRequester = target.GetRelationshipWithCitizen (this.startedBy).lordRelationship;
				RELATIONSHIP_STATUS relationshipWithTarget = target.GetRelationshipWithCitizen (targetKingdom.king).lordRelationship;

				if (envoy.citizen.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
					successRate += 5;
				}

				if (relationshipWithRequester == RELATIONSHIP_STATUS.WARM) {
					successRate += 5;
				} else if (relationshipWithRequester == RELATIONSHIP_STATUS.FRIEND) {
					successRate += 20;
				} else if (relationshipWithRequester == RELATIONSHIP_STATUS.ALLY) {
					successRate += 35;
				} 

				if (relationshipWithTarget == RELATIONSHIP_STATUS.COLD) {
					successRate += 5;
				} else if (relationshipWithTarget == RELATIONSHIP_STATUS.ENEMY) {
					successRate += 20;
				} else if (relationshipWithTarget == RELATIONSHIP_STATUS.RIVAL) {
					successRate += 35;
				} 

				int chanceForSuccess = Random.Range (0, 100);
				if (chanceForSuccess < successRate) {
					//target king will start invasion plan
				}
			}
		}
	}
}
