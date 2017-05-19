﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JoinWar : GameEvent {

	private Citizen _candidateForAlliance;
	private Envoy _envoyToSend;
	private Kingdom _kingdomToAttack;
	private List<Citizen> _uncovered;

	#region getters/setters
	public Citizen candidateForAlliance{
		get{ return this._candidateForAlliance; }
	}

	public List<Citizen> uncovered{
		get{ return this._uncovered; }
	}

	public Kingdom kingdomToAttack{
		get { return this._kingdomToAttack; }
	}
	#endregion

	public JoinWar(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen _candidateForAlliance, Envoy _envoyToSend, Kingdom _kingdomToAttack) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.JOIN_WAR_REQUEST;
		this.description = startedBy.name + " is looking for allies against kingdom " + kingdomToAttack.name;
		this.durationInDays = 4;
		this.remainingDays = this.durationInDays;

		this._candidateForAlliance = _candidateForAlliance;
		this._envoyToSend = _envoyToSend;
		this._kingdomToAttack = _kingdomToAttack;
		this._uncovered = new List<Citizen>();

		if(this._envoyToSend != null){
			this.startedBy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.startedBy.name + " sent " + this._envoyToSend.citizen.name
			+ " to " + candidateForAlliance.name + " to persuade him/her to join his/her Invasion Plan against " + this.kingdomToAttack.king.name, HISTORY_IDENTIFIER.NONE));
		}
		this.candidateForAlliance.city.hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	#region overrides
	internal override void PerformAction(){
		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (this._envoyToSend.citizen.isDead) {
			this.resolution = this._envoyToSend.citizen.name + " died before the event could finish.";
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
		if (EventManager.Instance.GetEventsStartedByKingdom (this.candidateForAlliance.city.kingdom, new EVENT_TYPES[]{EVENT_TYPES.INVASION_PLAN}).Where(x => x.isActive).Count() > 0) {
			this.resolution = this.candidateForAlliance.city.kingdom.name + " did not join " + this.startedByKingdom.name + " in his war against " + this.kingdomToAttack.name + 
				" because they already have other invasion plans.";
			this.DoneEvent ();
			return;
		}
		War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.candidateForAlliance.city.kingdom, this.kingdomToAttack);
		if (warEvent != null && warEvent.isAtWar) {
			this.resolution = this.candidateForAlliance.city.kingdom.name + " did not join " + this.startedByKingdom.name + " in his war against " + this.kingdomToAttack.name + 
				" because " + this.candidateForAlliance.city.kingdom.name + " is already at war with " + this.kingdomToAttack.name + ".";
			this.DoneEvent ();
			return;
		}

		if (this.remainingDays > 0) {
			this.remainingDays -= 1;
		} 
		if (this.remainingDays <= 0) {			
			int successRate = 15;
			RELATIONSHIP_STATUS relationshipWithRequester = candidateForAlliance.GetRelationshipWithCitizen (this.startedBy).lordRelationship;
			RELATIONSHIP_STATUS relationshipWithTarget = candidateForAlliance.GetRelationshipWithCitizen (kingdomToAttack.king).lordRelationship;

			if (this._envoyToSend.citizen.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
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
			} else if (relationshipWithTarget == RELATIONSHIP_STATUS.WARM) {
				successRate -= 5;
			} else if (relationshipWithTarget == RELATIONSHIP_STATUS.FRIEND) {
				successRate -= 20;
			} else if (relationshipWithTarget == RELATIONSHIP_STATUS.ALLY) {
				successRate -= 35;
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
				if (warEvent == null) {
					warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.candidateForAlliance, 
						this.candidateForAlliance.city.kingdom, this.kingdomToAttack);
				}

				if (!warEvent.isAtWar) {
					//Create new Invasion Plan against target kingdom
					warEvent.CreateInvasionPlan (this.candidateForAlliance.city.kingdom, this);
//						InvasionPlan newInvasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
//							this.candidateForAlliance, this.candidateForAlliance.city.kingdom, this.kingdomToAttack, this);
					this.candidateForAlliance.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
						this.candidateForAlliance.city.name + " has joined " + this.startedByCity.name + " in it's war against kingdom " + this.kingdomToAttack.name, HISTORY_IDENTIFIER.NONE));

					this.resolution = this.candidateForAlliance.city.name + " has joined " + this.startedByCity.name + " in it's war against kingdom " + this.kingdomToAttack.name;
				}
			}
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this._envoyToSend.inAction = false;
		this.endDay = GameManager.Instance.days;
		this.endMonth = GameManager.Instance.month;
		this.endYear = GameManager.Instance.year;
	}
	#endregion

	internal void AddCitizenThatUncoveredEvent(Citizen citizen){
		this._uncovered.Add(citizen);
	}
}
