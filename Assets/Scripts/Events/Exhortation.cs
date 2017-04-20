using UnityEngine;
using System.Collections;

public class Exhortation : GameEvent {

	public Citizen citizenSent;
	public Citizen targetCitizen;
	internal int successRate = 0;
	internal PowerGrab powerGrabThatStartedEvent;

	public Exhortation(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen citizenSent, Citizen targetCitizen, PowerGrab powerGrabThatStartedEvent) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXHORTATION;
		this.eventStatus = EVENT_STATUS.EXPOSED;
		this.description = startedBy.name + " is trying to gather support.";
		this.durationInWeeks = 2;
		this.remainingWeeks = this.durationInWeeks;
		this.citizenSent = citizenSent;
		this.targetCitizen = targetCitizen;
		this.powerGrabThatStartedEvent = powerGrabThatStartedEvent;

		this.targetCitizen.city.hexTile.AddEventOnTile(this);
		targetCitizen.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, 
			this.startedBy.name + " from " + this.startedByCity.name + " has started an Exhoration event to pursuade " + this.targetCitizen.name + "." , HISTORY_IDENTIFIER.NONE));

		this.successRate = 65;
		if (this.citizenSent.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
			this.successRate += 10;
		}
		if (this.citizenSent.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
			this.successRate += 10;
		}
		if (this.citizenSent.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
			this.successRate -= 10;
		}
		if (this.citizenSent.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
			this.successRate += 10;
		}
		if (this.citizenSent.miscTraits.Contains (MISC_TRAIT.LOYAL)) {
			this.successRate -= 55;
		}

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		} else {
			int chance = Random.Range(0, 100);
			if (chance < this.successRate) {
				this.targetCitizen.supportedCitizen = this.startedBy;
				this.targetCitizen.supportExpirationWeek = GameManager.Instance.week;
				this.targetCitizen.supportExpirationMonth = GameManager.Instance.month;
				this.targetCitizen.supportExpirationYear = GameManager.Instance.year + 2;
				powerGrabThatStartedEvent.exhortedCitizens.Add(this.targetCitizen);
				this.startedBy.history.Add (new History (startMonth, startWeek, startYear, this.startedBy.name + " was successful in influencing " + this.targetCitizen.name + ".", HISTORY_IDENTIFIER.NONE));
			}else{
				this.startedBy.history.Add (new History (startMonth, startWeek, startYear, this.startedBy.name + " was unsuccessful in influencing " + this.targetCitizen.name + ".", HISTORY_IDENTIFIER.NONE));
			}
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		if (citizenSent.role == ROLE.ENVOY) {
			((Envoy)citizenSent.assignedRole).inAction = false;
		}
	}
}
