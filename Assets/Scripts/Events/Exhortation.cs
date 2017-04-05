using UnityEngine;
using System.Collections;

public class Exhortation : GameEvent {

	public Citizen citizenSent;
	public Citizen targetCitizen;

	public Exhortation(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen citizenSent, Citizen targetCitizen) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXHORTATION;
		this.eventStatus = EVENT_STATUS.EXPOSED;
		this.description = startedBy.name + " is trying to gather support.";
		this.durationInWeeks = 2;
		this.remainingWeeks = this.durationInWeeks;
		this.citizenSent = citizenSent;
		this.targetCitizen = targetCitizen;
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		} else {
			int successRate = 65;
			if (this.citizenSent.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
				successRate += 10;
			}
			if (this.citizenSent.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
				successRate += 10;
			}
			if (this.citizenSent.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
				successRate -= 10;
			}
			if (this.citizenSent.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
				successRate += 10;
			}
			if (this.citizenSent.miscTraits.Contains (MISC_TRAIT.LOYAL)) {
				successRate -= 55;
			}
			int chance = Random.Range(0, 100);
			if (chance < successRate) {
				this.targetCitizen.supportedCitizen = this.startedBy;
				this.targetCitizen.supportExpirationWeek = GameManager.Instance.week;
				this.targetCitizen.supportExpirationMonth = GameManager.Instance.month;
				this.targetCitizen.supportExpirationYear = GameManager.Instance.year + 2;
			}
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
	}
}
