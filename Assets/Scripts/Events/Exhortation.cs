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
		this.durationInDays = 2;
		this.remainingDays = this.durationInDays;
		this.citizenSent = citizenSent;
		this.targetCitizen = targetCitizen;
		this.powerGrabThatStartedEvent = powerGrabThatStartedEvent;
		this.successRate = 65;

		if (this.targetCitizen.hasTrait(TRAIT.HONEST)) {
			this.successRate += 10;
		}

		Debug.LogError (this.description);
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
//		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (this.citizenSent.isDead) {
			this.resolution = this.citizenSent.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (this.remainingDays > 0) {
			this.remainingDays -= 1;
		} else {
			if (GameManager.Instance.month < this.targetCitizen.monthSupportCanBeChanged && GameManager.Instance.year == this.targetCitizen.yearSupportStarted) {
				//fail
//				Debug.LogError(this.targetCitizen.name + " can't changed support to " + this.startedBy.name + " (" + ((MONTH)GameManager.Instance.month) + ", " + GameManager.Instance.days + " " + GameManager.Instance.year + ")");
				return;
			}
			int chance = Random.Range(0, 100);
			if (chance < this.successRate) {
				this.targetCitizen.supportedCitizen = this.startedBy;
				this.targetCitizen.supportExpirationWeek = GameManager.Instance.days;
				this.targetCitizen.supportExpirationMonth = GameManager.Instance.month;
				this.targetCitizen.supportExpirationYear = GameManager.Instance.year + 2;

				this.targetCitizen.monthSupportCanBeChanged = GameManager.Instance.month + 1;
				this.targetCitizen.yearSupportStarted = GameManager.Instance.year;

//				Debug.LogError(this.targetCitizen.name + " changed support to " + this.startedBy.name + " (" + ((MONTH)GameManager.Instance.month) + ", " + GameManager.Instance.days + " " + GameManager.Instance.year + ")");

				powerGrabThatStartedEvent.exhortedCitizens.Add(this.targetCitizen);
			}
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
        base.DoneEvent();
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
//		if (citizenSent.role == ROLE.ENVOY) {
//			((Envoy)citizenSent.assignedRole).inAction = false;
//		}
	}
}
