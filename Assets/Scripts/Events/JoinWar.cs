using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JoinWar : GameEvent {

	private Citizen _candidateForAlliance;
	private Envoy _envoyToSend;
	private Kingdom _kingdomToAttack;
	private List<Citizen> _uncovered;

	private InvasionPlan _invasionPlanThatStartedEvent;
	private War warEvent = null;

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

	public InvasionPlan invasionPlanThatStartedEvent{
		get { return this._invasionPlanThatStartedEvent; }
	}

	public Envoy envoyToSend{
		get { return this._envoyToSend; }
		set {this._envoyToSend = value; }
	}
	#endregion

	public JoinWar(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen _candidateForAlliance, Envoy _envoyToSend, Kingdom _kingdomToAttack, 
		InvasionPlan _invasionPlanThatStartedEvent) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.JOIN_WAR_REQUEST;
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this._candidateForAlliance = _candidateForAlliance;
		this._envoyToSend = _envoyToSend;
		this._kingdomToAttack = _kingdomToAttack;
		this._uncovered = new List<Citizen>();
		this._invasionPlanThatStartedEvent = _invasionPlanThatStartedEvent;
		this.warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.candidateForAlliance.city.kingdom, this.kingdomToAttack);
			
		Log startLog = _invasionPlanThatStartedEvent.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			               "Events", "War", "join_war_start");
		startLog.AddToFillers (this.startedBy, this.startedBy.name);
		startLog.AddToFillers (_envoyToSend.citizen, _envoyToSend.citizen.name);
		startLog.AddToFillers (this._candidateForAlliance, this._candidateForAlliance.name);
		startLog.AddToFillers (this._kingdomToAttack, this._kingdomToAttack.name);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	#region overrides
	internal override void PerformAction(){
		if(!this.invasionPlanThatStartedEvent.isActive){
			//Join War Request is cancelled since the Invasion Plan is cancelled
			this.DoneEvent();
			return;
		}
		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
//		if (this._envoyToSend.citizen.isDead) {
//			this.resolution = this._envoyToSend.citizen.name + " died before the event could finish.";
//			this.DoneEvent();
//			return;
//		}
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
		if (this.warEvent != null && this.warEvent.isAtWar) {
			this.resolution = this.candidateForAlliance.city.kingdom.name + " did not join " + this.startedByKingdom.name + " in his war against " + this.kingdomToAttack.name + 
				" because " + this.candidateForAlliance.city.kingdom.name + " is already at war with " + this.kingdomToAttack.name + ".";
			this.DoneEvent ();
			return;
		}
		/*if (this.remainingDays <= 0) {
			int successRate = 15;
			RELATIONSHIP_STATUS relationshipWithRequester = candidateForAlliance.GetRelationshipWithCitizen (this.startedBy).lordRelationship;
			RELATIONSHIP_STATUS relationshipWithTarget = candidateForAlliance.GetRelationshipWithCitizen (kingdomToAttack.king).lordRelationship;

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
				RelationshipKings relationship = this.startedBy.GetRelationshipWithCitizen (this.candidateForAlliance);
				relationship.AdjustLikeness (5, this);

				if (warEvent == null) {
					warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.candidateForAlliance, 
						this.candidateForAlliance.city.kingdom, this.kingdomToAttack);
				}

				if (!warEvent.isAtWar) {
					Log joinWarSuccessLog = _invasionPlanThatStartedEvent.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
						"Events", "War", "join_war_success");
					joinWarSuccessLog.AddToFillers (this._candidateForAlliance, this._candidateForAlliance.name);
					joinWarSuccessLog.AddToFillers (this.startedBy, this.startedBy.name);
					joinWarSuccessLog.AddToFillers (this._kingdomToAttack, this._kingdomToAttack.name);

					//Create new Invasion Plan against target kingdom
					warEvent.CreateInvasionPlan (this.candidateForAlliance.city.kingdom, this);

					this.resolution = this.candidateForAlliance.city.name + " has joined " + this.startedByCity.name + " in it's war against kingdom " + this.kingdomToAttack.name;
					this.DoneEvent();
					return;
				}
			}

			//fail
			Log joinWarFailLog = _invasionPlanThatStartedEvent.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
				"Events", "War", "join_war_fail");
			joinWarFailLog.AddToFillers (this._candidateForAlliance, this._candidateForAlliance.name);
			joinWarFailLog.AddToFillers (this.startedBy, this.startedBy.name);
			this.DoneEvent();
		}*/
	}
	internal override void DoneCitizenAction(Envoy citizen){
		int successRate = 15;
		RELATIONSHIP_STATUS relationshipWithRequester = candidateForAlliance.GetRelationshipWithCitizen (this.startedBy).lordRelationship;
		RELATIONSHIP_STATUS relationshipWithTarget = candidateForAlliance.GetRelationshipWithCitizen (kingdomToAttack.king).lordRelationship;

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
			RelationshipKings relationship = this.startedBy.GetRelationshipWithCitizen (this.candidateForAlliance);
			relationship.AdjustLikeness (5, this);

			if (this.warEvent == null) {
				this.warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.candidateForAlliance, 
					this.candidateForAlliance.city.kingdom, this.kingdomToAttack);
			}

			if (!this.warEvent.isAtWar) {
				Log joinWarSuccessLog = _invasionPlanThatStartedEvent.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
					"Events", "War", "join_war_success");
				joinWarSuccessLog.AddToFillers (this._candidateForAlliance, this._candidateForAlliance.name);
				joinWarSuccessLog.AddToFillers (this.startedBy, this.startedBy.name);
				joinWarSuccessLog.AddToFillers (this._kingdomToAttack, this._kingdomToAttack.name);

				//Create new Invasion Plan against target kingdom
				warEvent.CreateInvasionPlan (this.candidateForAlliance.city.kingdom, this);

				this.resolution = this.candidateForAlliance.city.name + " has joined " + this.startedByCity.name + " in it's war against kingdom " + this.kingdomToAttack.name;
				this.DoneEvent();
				return;
			}
		}

		//fail
		Log joinWarFailLog = _invasionPlanThatStartedEvent.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			"Events", "War", "join_war_fail");
		joinWarFailLog.AddToFillers (this._candidateForAlliance, this._candidateForAlliance.name);
		joinWarFailLog.AddToFillers (this.startedBy, this.startedBy.name);
		this.DoneEvent();
	}
	internal override void DeathByOtherReasons(){
//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail_died");
//		newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail_died");
//		newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		if(this._envoyToSend != null){
			this._envoyToSend.DestroyGO ();
		}
		this.isActive = false;
//		this._envoyToSend.inAction = false;
		this.endDay = GameManager.Instance.days;
		this.endMonth = GameManager.Instance.month;
		this.endYear = GameManager.Instance.year;
	}
	#endregion

	internal void AddCitizenThatUncoveredEvent(Citizen citizen){
		this._uncovered.Add(citizen);
	}
}
