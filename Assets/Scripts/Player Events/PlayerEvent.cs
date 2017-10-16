using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerEvent {

	public int id;
	public PLAYER_EVENT eventType;
	public string name;

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

	private DateTime _startDate;

	internal bool relationshipHasDeteriorated;
	internal bool relationshipHasImproved;

	internal List<Kingdom> affectedKingdoms;

	protected Citizen _startedBy;
	protected WAR_TRIGGER _warTrigger;

	public Citizen startedBy {
		get {
			return this._startedBy;
		}
	}

	public WAR_TRIGGER warTrigger {
		get {
			return this._warTrigger;
		}
	}

	public DateTime startDate{
		get { return this._startDate; }
	}

	public PlayerEvent(int startWeek, int startMonth, int startYear, Citizen startedBy){
		this.id = Utilities.SetID(this);
		this.name = "Player Event";
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
		this._warTrigger = WAR_TRIGGER.NONE;
		this.relationshipHasDeteriorated = false;
		this.relationshipHasImproved = false;
		this.logs = new List<Log>();
		this.affectedKingdoms = new List<Kingdom> ();
		this._startDate = new DateTime (this.startYear, this.startMonth, this.startDay);
		if(this._startedBy != null){
			this.startedByKingdom = _startedBy.city.kingdom;
			this.startedByCity = _startedBy.city;
		}
		//		Debug.Log("New Event was created!");
	}

	#region virtual methods
	internal virtual void PerformAction(){}

	internal virtual void DoneCitizenAction(Citizen citizen){
		if(citizen.assignedRole.targetCity == null){
			if(citizen.assignedRole.targetLocation.lair == null){
				CancelEvent();
				return;
			}
		}else{
			if (citizen.assignedRole.targetCity.isDead) {
				CancelEvent();
				return;
			}
		}
		if(!this.isActive){
			return;
		}
		//CheckIfCitizenIsCarryingPlague(citizen);
	}

	internal virtual void CancelEvent(){
		Debug.Log (this.eventType.ToString() + " EVENT IS CANCELLED");
		this.isActive = false;
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;
	}

	internal virtual void DoneEvent(){
		Debug.Log (this.eventType.ToString () + " EVENT IS DONE");
		this.isActive = false;
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;

//		if(UIManager.Instance.currentlyShowingLogObject != null){
//			UIManager.Instance.eventLogsQueue.Add (this);
//		}else{
//			UIManager.Instance.Pause ();
//			UIManager.Instance.ShowEventLogs (this);
//		}
	}
	internal virtual void DeathByOtherReasons(){}
	internal virtual void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		deadCitizen.Death(DEATH_REASONS.BATTLE);
	}
	internal virtual void DeathByMonster(Monster monster, Citizen deadCitizen){
		deadCitizen.Death(DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	#endregion

	/*
	 * Create new log for this Event.
	 * rather than outside. Seems cleaner that way.
	 * */
	internal Log CreateNewLogForEvent(int month, int day, int year, string category, string file, string key){
		Log newLog = new Log (month, day, year, category, file, key);
		this.logs.Add (newLog);
		return newLog;
	}


	internal bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}
	//When an event has been constructed and created
	internal void PlayerEventIsCreated(){
		UIManager.Instance.ShowPlayerEventsOfType (this);
	}
}
