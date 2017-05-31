using UnityEngine;
using System.Collections;

public class Sabotage : GameEvent {

	public Sabotage(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom invitedKingdom, Envoy visitor) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.STATE_VISIT;
		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

//		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
//		newLogTitle.AddToFillers (visitor.citizen, visitor.citizen.name);
//		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
//
//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
//		newLog.AddToFillers (visitor.citizen, visitor.citizen.name);
//		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
//		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);

		EventManager.Instance.AddEventToDictionary (this);
		this.EventIsCreated ();

	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;
	}
}
