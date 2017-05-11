using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JoinWar : GameEvent {

	public Citizen candidateForAlliance;
	public Envoy envoyToSend;
	public Kingdom kingdomToAttack;
	public List<Citizen> uncovered;

	public JoinWar(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen candidateForAlliance, Envoy envoyToSend, Kingdom kingdomToAttack) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.JOIN_WAR_REQUEST;
		this.description = startedBy.name + " is looking for allies against kingdom " + kingdomToAttack.name;
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;

		this.candidateForAlliance = candidateForAlliance;
		this.envoyToSend = envoyToSend;
		this.kingdomToAttack = kingdomToAttack;
		this.uncovered = new List<Citizen>();

		if(this.envoyToSend != null){
			this.startedBy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.startedBy.name + " sent " + this.envoyToSend.citizen.name
			+ " to " + candidateForAlliance.name + " to persuade him/her to join his/her Invasion Plan against " + this.kingdomToAttack.king.name, HISTORY_IDENTIFIER.NONE));
		}
		this.candidateForAlliance.city.hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (this.envoyToSend.citizen.isDead) {
			this.resolution = this.envoyToSend.citizen.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (this.candidateForAlliance.isDead) {
			this.resolution = this.candidateForAlliance.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (!candidateForAlliance.isKing) {
			this.resolution = this.candidateForAlliance.name + " was overthrown before " + this.startedBy.name + " could invite him to join his war against " + this.kingdomToAttack.name;
			this.DoneEvent();
			return;
		}
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		} 
		if (this.remainingWeeks <= 0) {
			if (EventManager.Instance.GetEventsOfTypePerKingdom (this.candidateForAlliance.city.kingdom, EVENT_TYPES.INVASION_PLAN).Where(x => x.isActive).Count() > 0 ||
				KingdomManager.Instance.GetWarBetweenKingdoms(this.candidateForAlliance.city.kingdom, kingdomToAttack) != null) {
				//fail
				this.resolution = this.candidateForAlliance.city.kingdom.name + " did not join " + this.startedByKingdom.name + " in his war against " + this.kingdomToAttack.name + 
					" because they already have other invasion plans.";
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
					RelationshipKings relationship = this.startedBy.GetRelationshipWithCitizen(this.candidateForAlliance);
					relationship.AdjustLikeness(5, this);
					relationship.relationshipHistory.Add (new History (
						GameManager.Instance.month,
						GameManager.Instance.days,
						GameManager.Instance.year,
						this.candidateForAlliance.name + " accepted a join war request from " + this.startedBy.name + " against " + this.kingdomToAttack.name,
						HISTORY_IDENTIFIER.KING_RELATIONS,
						true
					));
					InvasionPlan newInvasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
						this.candidateForAlliance, this.candidateForAlliance.city.kingdom, this.kingdomToAttack, this);
					this.candidateForAlliance.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
						this.candidateForAlliance.city.name + " has joined " + this.startedByCity.name + " in it's war against kingdom " + this.kingdomToAttack.name , HISTORY_IDENTIFIER.NONE));

					this.resolution = this.candidateForAlliance.city.name + " has joined " + this.startedByCity.name + " in it's war against kingdom " + this.kingdomToAttack.name;
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
		envoyToSend.inAction = false;
	}
}
