using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Secession : GameEvent {

	internal Governor governor;
	internal Kingdom sourceKingdom;
	internal List<City> alreadyVisitedCities;
	internal List<City> joiningCities;
	internal Envoy convincer;
	internal City targetCity;
	public Secession(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SECESSION;
		//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.governor = (Governor)startedBy.assignedRole;
		this.sourceKingdom = startedBy.city.kingdom;
		this.alreadyVisitedCities = new List<City> ();
		this.convincer = null;
		this.targetCity = null;
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
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			DoneEvent ();
		}else{
			if(this.targetCity == null){
				SendEnvoy ();
			}
		}
	}
	internal override void DoneCitizenAction (Citizen citizen){
		base.DoneCitizenAction(citizen);
		if(this.convincer != null){
			if(citizen.id == this.convincer.citizen.id){
				//Convince governor to join
				ConvinceGovernor();
			}
		}
		this.targetCity = null;
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
		this.convincer.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.SplitKingdom ();
		//Generate new kingdom
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.targetCity = null;
	}
	#endregion

	private void SendEnvoy(){
		List<City> candidateCities = this.governor.citizen.city.kingdom.cities.Where (x => !this.alreadyVisitedCities.Contains (x)).ToList();
		if(candidateCities != null && candidateCities.Count > 0){
			this.targetCity = candidateCities [UnityEngine.Random.Range (0, candidateCities.Count)];
		}
		if (this.targetCity == null) {
			return;
		}

		Citizen chosenCitizen = this.governor.citizen.city.CreateAgent (ROLE.ENVOY, this.eventType, this.targetCity.hexTile, this.remainingDays);
		if(chosenCitizen == null){
			this.targetCity = null;
			return;
		}
		this.alreadyVisitedCities.Add (this.targetCity);
		chosenCitizen.assignedRole.Initialize (this);
	}
	private void ConvinceGovernor(){
		if(this.targetCity.governor != null){
			if(((Governor)this.targetCity.governor.assignedRole).loyalty < 0){
				this.joiningCities.Add (this.targetCity);
			}
		}
	}
	private void SplitKingdom(){
	}
}
