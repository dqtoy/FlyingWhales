using UnityEngine;
using System.Collections;

public class JoinWar : GameEvent {

	public Citizen candidateForAlliance;
	public Envoy envoyToSend;
	public Kingdom kingdomToAttack;

	public JoinWar(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen candidateForAlliance, Envoy envoyToSend, Kingdom kingdomToAttack) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.JOIN_WAR_REQUEST;
		this.description = startedBy.name + " is looking for allies against kingdom " + kingdomToAttack.name;
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;

		this.candidateForAlliance = candidateForAlliance;
		this.envoyToSend = envoyToSend;
		this.kingdomToAttack = kingdomToAttack;

		if(this.envoyToSend != null){
			this.startedBy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, this.startedBy.name + " sent " + this.envoyToSend.citizen.name
			+ " to " + candidateForAlliance.name + " to persuade him/her to join his/her Invasion Plan against " + this.kingdomToAttack.king.name, HISTORY_IDENTIFIER.NONE));
		}
		this.candidateForAlliance.city.hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		} else {
			if (EventManager.Instance.GetEventsOfTypePerKingdom (this.candidateForAlliance.city.kingdom, EVENT_TYPES.INVASION_PLAN).Count > 0) {
				//fail
			} else {
				int successRate = 15;
				RELATIONSHIP_STATUS relationshipWithRequester = candidateForAlliance.GetRelationshipWithCitizen (this.startedBy).lordRelationship;
				RELATIONSHIP_STATUS relationshipWithTarget = candidateForAlliance.GetRelationshipWithCitizen (kingdomToAttack.king).lordRelationship;

				if (envoyToSend.citizen.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
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
					this.startedBy.GetRelationshipWithCitizen(this.candidateForAlliance).AdjustLikeness(5);
					InvasionPlan newInvasionPlan = new InvasionPlan(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, 
						this.candidateForAlliance, this.candidateForAlliance.city.kingdom, this.kingdomToAttack);
					EventManager.Instance.AddEventToDictionary (newInvasionPlan);

				}
				this.DoneEvent();
			}
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
	}
}
