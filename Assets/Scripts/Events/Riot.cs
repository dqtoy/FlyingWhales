using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Riot : GameEvent {

	public Rebel rebel;
	internal Kingdom sourceKingdom;
	public Riot(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RIOT;
		this.name = "Riot";
		//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = startedBy.city.kingdom;
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError (startedBy.name + " has started a riot in " + this.sourceKingdom.name);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Riot", "event_title");
		newLogTitle.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Riot", "start");
		newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		//		EventManager.Instance.AddEventToDictionary (this);
		//		this.EventIsCreated ();

		this.EventIsCreated ();
	}

	#region Overrides
	internal override void PerformAction (){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			DoneEvent ();
		}else{
			if(!this.sourceKingdom.isAlive()){
				CancelEvent ();
				return;
			}
			AttemptToDestroyStructure ();
		}
	}
//	internal override void DoneCitizenAction (Citizen citizen){
//		base.DoneCitizenAction(citizen);
//		if(this.saboteur != null){
//			if(citizen.id == this.saboteur.citizen.id){
//				AttemptToSabotage();
//			}
//		}
//	}
//	internal override void DeathByOtherReasons(){
//		this.DoneEvent();
//	}
//	internal override void DeathByAgent(Citizen citizen){
//		this.saboteur.citizen.Death (DEATH_REASONS.BATTLE);
//		this.DoneEvent();
//	}
	internal override void DoneEvent(){
		base.DoneEvent();
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Riot", "event_end");
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
	private void AttemptToDestroyStructure(){
		int chance = UnityEngine.Random.Range(0, 100);
		if(chance < 3){
			List<City> candidates = this.sourceKingdom.cities.Where (x => x.structures.Count > 0).ToList ();
			if(candidates != null && candidates.Count > 0){
				City chosenCity = candidates[UnityEngine.Random.Range(0, candidates.Count)];
				DestroyStructure (chosenCity);
			}
		}
	}

	private void DestroyStructure(City city){
		//Destroy a structure
		Debug.Log("DESTROYED A STRUCTURE IN " + city.name);
		city.RemoveTileFromCity(city.structures[city.structures.Count - 1]);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Riot", "destroy_settlement");
		newLog.AddToFillers (city, city.name, LOG_IDENTIFIER.CITY_1);
	}
}
