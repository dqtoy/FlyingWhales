using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameEvent {

	public int id;
	public EVENT_TYPES eventType;
	public EVENT_STATUS eventStatus;

	public int startDay;
	public int startMonth;
	public int startYear;

	public int durationInDays;
	public int remainingDays;

	public int endDay;
	public int endMonth;
	public int endYear;

	public string description;
	public string resolution;
	public Kingdom startedByKingdom;
	public City startedByCity;
	public bool isActive;
	public List<Log> logs;

	protected Citizen _startedBy;

	public Citizen startedBy {
		get {
			return this._startedBy;
		}
	}

	public GameEvent(int startWeek, int startMonth, int startYear, Citizen startedBy){
		this.id = Utilities.SetID(this);
		this.eventStatus = EVENT_STATUS.EXPOSED;
		this.startDay = startWeek;
		this.startMonth = startMonth;
		this.startYear = startYear;
		this._startedBy = startedBy;
		this.durationInDays = 0;
		this.remainingDays = 0;
		this.endDay = 0;
		this.endMonth = 0;
		this.endYear = 0;
		this.description = "";
		this.resolution = "";
		this.isActive = true;
		this.logs = new List<Log>();
		if(this._startedBy != null){
			this.startedByKingdom = _startedBy.city.kingdom;
			this.startedByCity = _startedBy.city;
		}
		Debug.Log("New Event was created!");
	}

	#region virtual methods
	internal virtual void PerformAction(){}

	internal virtual void DoneCitizenAction(Envoy citizen){}

	internal virtual void DoneEvent(){
		Debug.Log ("Game Event Ended!");
		EventManager.Instance.onGameEventEnded.Invoke(this);
	} 
	#endregion

	internal Log CreateNewLogForEvent(int month, int day, int year, string category, string file, string key){
		Log newLog = new Log (month, day, year, category, file, key);
		this.logs.Add (newLog);
		Debug.Log ("LALALALALALALA " + Utilities.LogReplacer (newLog));
		return newLog;
	}

	internal virtual void CancelEvent(){}

	internal bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}
	internal List<Citizen> GetUnwantedGovernors(Citizen king){
		List<Citizen> unwantedGovernors = new List<Citizen> ();
		for(int i = 0; i < king.civilWars.Count; i++){
			if(king.civilWars[i].isGovernor){
				unwantedGovernors.Add (king.civilWars [i]);
			}
		}
		for(int i = 0; i < king.successionWars.Count; i++){
			if(king.successionWars[i].isGovernor){
				unwantedGovernors.Add (king.successionWars [i]);
			}
		}
		for(int i = 0; i < king.city.kingdom.cities.Count; i++){
			if(king.city.kingdom.cities[i].governor.supportedCitizen != null){
				unwantedGovernors.Add (king.city.kingdom.cities [i].governor);
			}
		}

		return unwantedGovernors;
	}
}
