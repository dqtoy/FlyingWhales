using UnityEngine;
using System.Collections;

public class AttackCity : GameEvent {

	internal General general;
	internal City targetCity;

	public AttackCity(int startWeek, int startMonth, int startYear, Citizen startedBy, General general, City targetCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ATTACK_CITY;
		//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.general = general;
		this.targetCity = targetCity;
		Debug.LogError (general.citizen.name + " of " + general.citizen.city.kingdom.name + " will attack " + targetCity.name);
//		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

		//		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
		//		newLogTitle.AddToFillers (visitor.citizen, visitor.citizen.name);
		//		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
		//		newLog.AddToFillers (visitor.citizen, visitor.citizen.name);
		//		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		//		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);

		//		EventManager.Instance.AddEventToDictionary (this);
		//		this.EventIsCreated ();

	}
	internal override void DoneCitizenAction (Citizen citizen){
		if(this.general != null){
			if(citizen.id == this.general.citizen.id){
				CombatManager.Instance.CityBattle (this.targetCity, this.general);
				this.general.citizen.Death (DEATH_REASONS.BATTLE);
			}
		}
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent ();
//		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}

	private void CheckTargetCity(){
		
	}
}
