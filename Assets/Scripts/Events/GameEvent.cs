using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameEvent {

	public int id;
	public EVENT_TYPES eventType;
	public EVENT_STATUS eventStatus;
	public string name;
    public GameEventAvatar gameEventAvatar;

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
	public bool isOneTime;
	public List<Log> logs;

	private DateTime _startDate;

	internal bool relationshipHasDeteriorated;
	internal bool relationshipHasImproved;
	internal GameObject goEventItem;

	protected Citizen _startedBy;
	protected WAR_TRIGGER _warTrigger;
	protected ASSASSINATION_TRIGGER_REASONS _assassinationTrigger;

	private List<Kingdom> _eventKingdoms;

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

	public ASSASSINATION_TRIGGER_REASONS assassinationTrigger {
		get {
			return this._assassinationTrigger;
		}
	}

	public DateTime startDate{
		get { return this._startDate; }
	}

	public List<Kingdom> eventKingdoms{
		get { return this._eventKingdoms; }
	}

	public GameEvent(int startWeek, int startMonth, int startYear, Citizen startedBy){
		this.id = Utilities.SetID(this);
		this.eventStatus = EVENT_STATUS.EXPOSED;
		this.name = "Game Event";
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
		this.isOneTime = false;
		this._warTrigger = WAR_TRIGGER.NONE;
		this._assassinationTrigger = ASSASSINATION_TRIGGER_REASONS.NONE;
		this.relationshipHasDeteriorated = false;
		this.relationshipHasImproved = false;
		this.goEventItem = null;
		this.logs = new List<Log>();
		this._eventKingdoms = new List<Kingdom> ();
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
        CheckIfCitizenIsCarryingPlague(citizen);
    }
	internal virtual void CancelEvent(){
		Debug.Log (this.eventType.ToString() + " EVENT IS CANCELLED");
        this.isActive = false;
        this.endMonth = GameManager.Instance.month;
        this.endDay = GameManager.Instance.days;
        this.endYear = GameManager.Instance.year;
		for (int i = 0; i < this._eventKingdoms.Count; i++) {
			this._eventKingdoms [i].RemoveActiveEvent (this);
		}
        if (this.goEventItem != null) {
            this.goEventItem.GetComponent<EventItem>().HasExpired();
        }
    }

	internal virtual void DoneEvent(){
		Debug.Log (this.eventType.ToString () + " EVENT IS DONE");
        this.isActive = false;
        this.endMonth = GameManager.Instance.month;
        this.endDay = GameManager.Instance.days;
        this.endYear = GameManager.Instance.year;

		for (int i = 0; i < this._eventKingdoms.Count; i++) {
			this._eventKingdoms [i].RemoveActiveEvent (this);
		}

		if(!this.isOneTime){
			if (this.startedBy != null && UIManager.Instance.currentlyShowingKingdom != null && this.logs.Count > 0) { //Kingdom Event
				if (this.startedByKingdom.id == UIManager.Instance.currentlyShowingKingdom.id) {
					if (!Utilities.eventsNotToShow.Contains(eventType)) {
						if (UIManager.Instance.currentlyShowingLogObject != null) {
							UIManager.Instance.eventLogsQueue.Add(this);
						} else {
							//					UIManager.Instance.Pause ();
							UIManager.Instance.ShowEventLogs(this);
						}
					}
				}
			}
		}

//		if(this.goEventItem != null){
//			this.goEventItem.GetComponent<EventItem> ().HasExpired ();
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
    internal virtual void OnCollectAvatarAction(Kingdom claimant) {
        gameEventAvatar.eventLocation.RemoveEventOnTile();
        DestroyEventAvatar();
    }
    #endregion

    internal void DestroyEventAvatar() {
        if (gameEventAvatar != null) {
            GameObject.Destroy(gameEventAvatar.gameObject);
        }
    }

    /*
    * A plague carrying citizen will spread the plague to its 
    * destination city (if it is not yet plagued) and infect a random settlement.
    * */
    internal void CheckIfCitizenIsCarryingPlague(Citizen citizen) {
        Plague plaguedCarriedByCitizen = citizen.assignedRole.plague;
        if (plaguedCarriedByCitizen != null) {
            City citizenTargetCity = citizen.assignedRole.targetCity;
            if (citizenTargetCity == null) {
                citizenTargetCity = citizen.currentLocation.city;
            }
            if (citizenTargetCity != null) {
                if (citizenTargetCity.plague == null) {
                    //City is not plagued yet
                    if (plaguedCarriedByCitizen.affectedKingdoms.Contains(citizenTargetCity.kingdom)) {
                        //Kingdom that city belongs to is already affected by this plague, only infect the city
                        HexTile infectedTile = plaguedCarriedByCitizen.PlagueACity(citizenTargetCity);
                        if (infectedTile != null) {
                            Log plagueSpreadByAgent = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_agent_spread");
                            plagueSpreadByAgent.AddToFillers(citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            plagueSpreadByAgent.AddToFillers(null, plaguedCarriedByCitizen.plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
                            plagueSpreadByAgent.AddToFillers(citizenTargetCity, citizenTargetCity.name, LOG_IDENTIFIER.CITY_1);

                        }
                    } else {
                        //Kingdom that city belongs to is not yet affected by this plague
                        if (citizenTargetCity.kingdom != null) {
                            if (citizenTargetCity.kingdom.plague == null) {
                                //Kingdom has no plague yet, infect that kingdom and destroy settlement
                                HexTile infectedTile = plaguedCarriedByCitizen.PlagueAKingdom(citizenTargetCity.kingdom, citizenTargetCity);
                                if (infectedTile != null) {
                                    Log plagueSpreadByAgent = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_agent_spread");
                                    plagueSpreadByAgent.AddToFillers(citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                    plagueSpreadByAgent.AddToFillers(null, plaguedCarriedByCitizen.plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
                                    plagueSpreadByAgent.AddToFillers(citizenTargetCity, citizenTargetCity.name, LOG_IDENTIFIER.CITY_1);
                                }
                            }
                        }

                    }
                } else {
                    //City is already plagued, just infect a settlement
                    HexTile infectedTile = plaguedCarriedByCitizen.InfectRandomSettlement(citizenTargetCity.structures);
                    if (infectedTile != null) {
                        Log plagueSpreadByAgent = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_agent_spread");
                        plagueSpreadByAgent.AddToFillers(citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        plagueSpreadByAgent.AddToFillers(null, plaguedCarriedByCitizen.plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
                        plagueSpreadByAgent.AddToFillers(citizenTargetCity, citizenTargetCity.name, LOG_IDENTIFIER.CITY_1);
                    }
                }
            }
        }
    }

    /*
	 * Create new log for this Event.
	 * rather than outside. Seems cleaner that way.
	 * */
    internal Log CreateNewLogForEvent(int month, int day, int year, string category, string file, string key){
		Log newLog = new Log (month, day, year, category, file, key);
		this.logs.Add (newLog);

		if(this.goEventItem != null){
			if(UIManager.Instance.currentlyShowingLogObject == null){
				this.goEventItem.GetComponent<EventItem> ().ActivateNewLogIndicator ();
			}else{
				if(UIManager.Instance.currentlyShowingLogObject is GameEvent){
					GameEvent gameEvent = (GameEvent)UIManager.Instance.currentlyShowingLogObject;
					if(gameEvent.id != this.id){
						this.goEventItem.GetComponent<EventItem> ().ActivateNewLogIndicator ();
					}
				}

			}
		}
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
	internal void EventIsCreated(Kingdom kingdom, bool isShow){
		if(isShow){
			if(!this.isOneTime){
				UIManager.Instance.ShowEventsOfType (this, kingdom);
			}else{
				if (kingdom.id == UIManager.Instance.currentlyShowingKingdom.id) {
					if (!Utilities.eventsNotToShow.Contains(eventType)) {
						if (UIManager.Instance.currentlyShowingLogObject != null) {
							UIManager.Instance.eventLogsQueue.Add(this);
						} else {
							//					UIManager.Instance.Pause ();
							UIManager.Instance.ShowEventLogs(this);
						}
					}
				}
			}
		}
		if(kingdom != null){
			AddEventInKingdom (kingdom);
		}
	}
    internal void SetStartedBy(Citizen startedBy) {
        _startedBy = startedBy;
        startedByCity = startedBy.city;
        startedByKingdom = startedBy.city.kingdom;
    }
	private void AddEventInKingdom(Kingdom kingdom){
		this._eventKingdoms.Add (kingdom);
		kingdom.AddActiveEvent (this);
	}
}
