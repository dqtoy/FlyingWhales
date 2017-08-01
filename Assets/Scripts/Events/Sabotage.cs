using UnityEngine;
using System.Collections;

public class Sabotage : GameEvent {

	internal GameEvent eventToSabotage;
	internal Envoy saboteur;
	public Sabotage(int startWeek, int startMonth, int startYear, Citizen startedBy, Envoy saboteur, GameEvent eventToSabotage) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SABOTAGE;
		this.name = "Sabotage";
//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.saboteur = saboteur;
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

//		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
//		newLogTitle.AddToFillers (visitor.citizen, visitor.citizen.name);
//		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
//
//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
//		newLog.AddToFillers (visitor.citizen, visitor.citizen.name);
//		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
//		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);

//		EventManager.Instance.AddEventToDictionary (this);
//		this.EventIsCreated ();

	}

	#region Overrides
	internal override void PerformAction (){
		CheckGameEvent();
	}
	internal override void DoneCitizenAction (Citizen citizen){
        base.DoneCitizenAction(citizen);
		if(this.saboteur != null){
			if(citizen.id == this.saboteur.citizen.id){
				AttemptToSabotage();
			}
		}
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
        base.DoneEvent();
//		if(this.saboteur != null){
//			this.saboteur.DestroyGO();
//		}
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
	private void AttemptToSabotage(){
		int chance = UnityEngine.Random.Range(0,100);
		if(chance < 20){
			if(this.eventToSabotage is StateVisit){
				StateVisit sv = (StateVisit) this.eventToSabotage;
				sv.EventIsSabotaged();
			}
			this.DoneEvent();
		}

	}

	private void CheckGameEvent(){
		if(this.eventToSabotage == null || !this.eventToSabotage.isActive){
			this.DoneEvent();
		}
	}
}
