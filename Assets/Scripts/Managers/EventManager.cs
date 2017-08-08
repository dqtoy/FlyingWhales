using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventManager : MonoBehaviour {
	public static EventManager Instance;

    public EVENT_TYPES[] playerPlacableEvents;

    [SerializeField] private Sprite[] eventAvatarSprites;

    private Dictionary <string, UnityEvent> eventDictionary;

	public Dictionary<EVENT_TYPES, List<GameEvent>> allEvents;

	/*
	 * Generic Events
	 * */
	public WeekEndedEvent onWeekEnd = new WeekEndedEvent();

	/*
	 * Kingdom Events
	 * */
	public NewKingdomEvent onCreateNewKingdomEvent = new NewKingdomEvent();
	public KingdomDiedEvent onKingdomDiedEvent = new KingdomDiedEvent();


	/*
	 * Citizen Events
	 * */
	public CitizenTurnActions onCitizenTurnActions = new CitizenTurnActions ();
	public CitizenDiedEvent onCitizenDiedEvent =  new CitizenDiedEvent();

	/*
	 * City Events
	 * */
	public CityEverydayTurnActions onCityEverydayTurnActions = new CityEverydayTurnActions();
	public CitizenMove onCitizenMove =  new CitizenMove();

	/*
	 * Campaign Events
	 * */
	public RegisterOnCampaign onRegisterOnCampaign = new RegisterOnCampaign();
	public DeathArmy onDeathArmy = new DeathArmy();
	public UnsupportCitizen onUnsupportCitizen = new UnsupportCitizen();
	public RemoveSuccessionWarCity onRemoveSuccessionWarCity = new RemoveSuccessionWarCity();
	public LookForLostArmies onLookForLostArmies = new LookForLostArmies ();
	public DeathToGhost onDeathToGhost = new DeathToGhost();
	public CheckGeneralEligibility onCheckGeneralEligibility = new CheckGeneralEligibility();
	public UpdatePath onUpdatePath = new UpdatePath();

	/*
	 * Game Events
	 * */
	public GameEventAction onGameEventAction = new GameEventAction();
	public GameEventEnded onGameEventEnded = new GameEventEnded();

	/*
	 * UI Events
	 * */
	public UpdateUI onUpdateUI = new UpdateUI();

	public Dictionary<EVENT_TYPES, int> eventDuration = new Dictionary<EVENT_TYPES, int>(){
		{EVENT_TYPES.BORDER_CONFLICT, 30},
		{EVENT_TYPES.DIPLOMATIC_CRISIS, 30},
		{EVENT_TYPES.INVASION_PLAN, 60},
		{EVENT_TYPES.JOIN_WAR_REQUEST, -1},
		{EVENT_TYPES.STATE_VISIT, 10},
		{EVENT_TYPES.ASSASSINATION, -1},
		{EVENT_TYPES.RAID, 5},
		{EVENT_TYPES.EXPANSION, -1},
        {EVENT_TYPES.TRADE, -1},
		{EVENT_TYPES.ATTACK_CITY, -1},
		{EVENT_TYPES.SABOTAGE, -1},
		{EVENT_TYPES.REINFORCEMENT, -1},
		{EVENT_TYPES.SECESSION, 60},
		{EVENT_TYPES.RIOT, 30},
		{EVENT_TYPES.REBELLION, -1},
        {EVENT_TYPES.REQUEST_PEACE, -1},
        {EVENT_TYPES.PLAGUE, -1},
		{EVENT_TYPES.SCOURGE_CITY, -1},
		{EVENT_TYPES.BOON_OF_POWER, -1},
		{EVENT_TYPES.PROVOCATION, -1},
		{EVENT_TYPES.EVANGELISM, -1},
		{EVENT_TYPES.SPOUSE_ABDUCTION, -1},
        {EVENT_TYPES.LYCANTHROPY, -1},
		{EVENT_TYPES.FIRST_AND_KEYSTONE, -1},
		{EVENT_TYPES.RUMOR, -1},
		{EVENT_TYPES.SLAVES_MERCHANT, -1},
		{EVENT_TYPES.HIDDEN_HISTORY_BOOK, -1},
        {EVENT_TYPES.HYPNOTISM, -1},
        {EVENT_TYPES.KINGDOM_HOLIDAY, -1},
		{EVENT_TYPES.SERUM_OF_ALACRITY, 30},
        {EVENT_TYPES.DEVELOP_WEAPONS, -1},
        {EVENT_TYPES.KINGS_COUNCIL, -1},
		{EVENT_TYPES.ALTAR_OF_BLESSING, -1},
        {EVENT_TYPES.ADVENTURE, -1},
        {EVENT_TYPES.EVIL_INTENT, -1},
        {EVENT_TYPES.ATTACK_LAIR, -1},
		{EVENT_TYPES.GREAT_STORM, -1},
		{EVENT_TYPES.SEND_RELIEF_GOODS, -1},

    };

	void Awake(){
		Instance = this;
		this.Init();
	}

	void Init (){
		if (eventDictionary == null){
			eventDictionary = new Dictionary<string, UnityEvent>();
		}
		if (allEvents == null) {
			allEvents = new Dictionary<EVENT_TYPES, List<GameEvent>>();
		}
	}

	/*
	 * Register an event to the allEvents Dictionary.
	 * */
	public void AddEventToDictionary(GameEvent gameEvent){
		if (allEvents.ContainsKey (gameEvent.eventType)) {
			allEvents [gameEvent.eventType].Add(gameEvent);
		} else {
			allEvents.Add (gameEvent.eventType, new List<GameEvent> (){ gameEvent });
		}
	}

	/*
	 * Get a list of all the events of a specific type (including done events).
	 * */
	public List<GameEvent> GetEventsOfType(EVENT_TYPES eventType){
		List<GameEvent> eventsOfType = new List<GameEvent>();
		if (this.allEvents.ContainsKey (eventType)) {
			eventsOfType = this.allEvents[eventType];
		}
		return eventsOfType;
	}

	/*
	 * Get a list of all the events started by a kingdom, 
	 * can pass event types to only get events of that type.
	 * */
	public List<GameEvent> GetEventsStartedByKingdom(Kingdom kingdom, EVENT_TYPES[] eventTypes, bool isActiveOnly = true){
		List<GameEvent> gameEventsOfTypePerKingdom = new List<GameEvent>();
		if (eventTypes.Contains (EVENT_TYPES.ALL)) {
			for (int i = 0; i < allEvents.Keys.Count; i++) {
				EVENT_TYPES currKey = allEvents.Keys.ElementAt(i);
				List<GameEvent> eventsOfType = allEvents[currKey];
				for (int j = 0; j < eventsOfType.Count; j++) {
					GameEvent currEvent = eventsOfType [j];
					if (currEvent.startedByKingdom != null && currEvent.startedByKingdom.id == kingdom.id) {
                        if (isActiveOnly) {
                            if (currEvent.isActive) {
                                gameEventsOfTypePerKingdom.Add(currEvent);
                            }
                        } else {
                            gameEventsOfTypePerKingdom.Add(currEvent);
                        }
					}
				}
			}
		} else {
			for (int i = 0; i < eventTypes.Length; i++) {
				EVENT_TYPES currentEvent = eventTypes [i];
				if (this.allEvents.ContainsKey (currentEvent)) {
					List<GameEvent> eventsOfType = this.allEvents [currentEvent];
					for (int j = 0; j < eventsOfType.Count; j++) {
                        GameEvent currEvent = eventsOfType[j];
                        if (currEvent.startedByKingdom != null && currEvent.startedByKingdom.id == kingdom.id) {
                            if (isActiveOnly) {
                                if (currEvent.isActive) {
                                    gameEventsOfTypePerKingdom.Add(currEvent);
                                }
                            } else {
                                gameEventsOfTypePerKingdom.Add(currEvent);
                            }
                        }
					}
				}
			}
		}
		return gameEventsOfTypePerKingdom;
	}

	/*
	 * Get a list of all events a kingdom is involved in.
     * Can pass an array of event types to return.
	 * */
	public List<GameEvent> GetAllEventsKingdomIsInvolvedIn(Kingdom kingdom, EVENT_TYPES[] eventTypes = null) {
		List<GameEvent> allGameEventsInKingdom = new List<GameEvent>();

        List<EVENT_TYPES> eventTypesToUse = new List<EVENT_TYPES>();
        if(eventTypes == null) {
            eventTypesToUse = this.allEvents.Keys.ToList();
        } else {
            if (eventTypes.Contains(EVENT_TYPES.ALL)) {
                eventTypesToUse = this.allEvents.Keys.ToList();
            } else {
                eventTypesToUse = eventTypes.ToList();
            }
        }
		for (int i = 0; i < eventTypesToUse.Count; i++) {
			EVENT_TYPES key = eventTypesToUse[i];
            if (this.allEvents.ContainsKey(key)) {
                List<GameEvent> eventsOfType = this.allEvents[key];
                if (eventsOfType.Count > 0) {
                    if (key == EVENT_TYPES.ADMIRATION) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Admiration currEvent = (Admiration)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.kingdom1.id == kingdom.id || currEvent.kingdom2.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.ASSASSINATION) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Assassination currEvent = (Assassination)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.assassinKingdom.id == kingdom.id || currEvent.targetCitizen.city.kingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.BORDER_CONFLICT) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            BorderConflict currEvent = (BorderConflict)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.kingdom1.id == kingdom.id || currEvent.kingdom2.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.DIPLOMATIC_CRISIS) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            DiplomaticCrisis currEvent = (DiplomaticCrisis)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.kingdom1.id == kingdom.id || currEvent.kingdom2.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.ESPIONAGE) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Espionage currEvent = (Espionage)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.sourceKingdom.id == kingdom.id || currEvent.targetKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.EXHORTATION) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Exhortation currEvent = (Exhortation)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.targetCitizen.city.kingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.INVASION_PLAN) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            InvasionPlan currEvent = (InvasionPlan)eventsOfType[j];
                            if (currEvent.sourceKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.JOIN_WAR_REQUEST) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            JoinWar currEvent = (JoinWar)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.MILITARIZATION) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Militarization currEvent = (Militarization)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.POWER_GRAB) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            PowerGrab currEvent = (PowerGrab)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.RAID) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Raid currEvent = (Raid)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.sourceKingdom.id == kingdom.id || currEvent.raidedCity.kingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.REQUEST_PEACE) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            RequestPeace currEvent = (RequestPeace)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.STATE_VISIT) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            StateVisit currEvent = (StateVisit)eventsOfType[j];
                            if (currEvent.startedByKingdom.id == kingdom.id || currEvent.invitedKingdom.id == kingdom.id || currEvent.inviterKingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.KINGDOM_WAR) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            War currEvent = (War)eventsOfType[j];
                            if (currEvent.kingdom1.id == kingdom.id || currEvent.kingdom2.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else if (key == EVENT_TYPES.TRADE) {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            Trade currEvent = (Trade)eventsOfType[j];
                            if (currEvent.sourceCity.kingdom.id == kingdom.id || currEvent.targetCity.kingdom.id == kingdom.id) {
                                allGameEventsInKingdom.Add(eventsOfType[j]);
                            }
                        }
                    } else {
                        for (int j = 0; j < eventsOfType.Count; j++) {
                            GameEvent currEvent = eventsOfType[j];
                            if ((currEvent.startedByKingdom != null && currEvent.startedByKingdom.id == kingdom.id) || 
                                (currEvent.startedBy != null && currEvent.startedBy.role == ROLE.KING && ((King)currEvent.startedBy.assignedRole).ownedKingdom.id == kingdom.id)) {
                                allGameEventsInKingdom.Add(currEvent);
                            }
                        }
                    }
                }
            }
			
		}
		return allGameEventsInKingdom;
	}

	/*
	 * Get a list of events started by a city
	 * */
	public List<GameEvent> GetAllEventsPerCity(City city){
		List<GameEvent> gameEventsOfCity = new List<GameEvent>();
		for (int i = 0; i < this.allEvents.Keys.Count; i++) {
			EVENT_TYPES currentKey = this.allEvents.Keys.ElementAt(i);
			List<GameEvent> gameEventsOfType = this.allEvents [currentKey];
			for (int j = 0; j < gameEventsOfType.Count; j++) {
				if (gameEventsOfType [j].startedByCity != null) {
					if (gameEventsOfType [j].startedByCity.id == city.id) {
						gameEventsOfCity.Add (gameEventsOfType [j]);
					}
				}
			}
		}
		return gameEventsOfCity;
	}

	/*
	 * Get a list of events that a citizen started
	 * */
	public List<GameEvent> GetAllEventsStartedByCitizen(Citizen citizen){
		List<GameEvent> gameEventsOfCitizen = new List<GameEvent>();
		for (int i = 0; i < this.allEvents.Keys.Count; i++) {
			EVENT_TYPES currentKey = this.allEvents.Keys.ElementAt (i);
			List<GameEvent> gameEventsOfType = this.allEvents [currentKey];
			for (int j = 0; j < gameEventsOfType.Count; j++) {
				if (gameEventsOfType[j].startedBy.id == citizen.id) {
					gameEventsOfCitizen.Add(gameEventsOfType [j]);
				}
			}
		}
		return gameEventsOfCitizen;
	}

	/*
	 * Get a list of events of type that a citizen started
	 * */
	public List<GameEvent> GetAllEventsStartedByCitizenByType(Citizen citizen, EVENT_TYPES eventType){
		List<GameEvent> gameEventsOfCitizen = new List<GameEvent>();
		if (this.allEvents.ContainsKey (eventType)) {
			List<GameEvent> gameEventsOfType = this.allEvents [eventType];
			for (int i = 0; i < gameEventsOfType.Count; i++) {
				if (gameEventsOfType[i].startedBy.id == citizen.id) {
					gameEventsOfCitizen.Add(gameEventsOfType[i]);
				}
			}
		}
		return gameEventsOfCitizen;
	}

	/*internal Citizen GetSpy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> spies = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.SPY) {
							if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
								spies.Add (kingdom.cities [i].citizens [j]);
							}
						}
					}
				}
			}
		}

		if(spies.Count > 0){
			int random = UnityEngine.Random.Range (0, spies.Count);
			((Spy)spies [random].assignedRole).inAction = true;
			return spies [random];
		}else{
//			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}*/

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

		return unwantedGovernors;
	}

    internal Sprite GetEventAvatarSprite(EVENT_TYPES eventType) {
        return eventAvatarSprites.Where(x => x.name == eventType.ToString()).FirstOrDefault();
    }
//	public static void StartListening (string eventName, UnityAction listener){
//		UnityEvent thisEvent = null;
//		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent)){
//			thisEvent.AddListener (listener);
//		} else {
//			thisEvent = new UnityEvent ();
//			thisEvent.AddListener (listener);
//			Instance.eventDictionary.Add (eventName, thisEvent);
//		}
//	}
//
//	public static void StopListening (string eventName, UnityAction listener){
//		if (Instance == null) return;
//		UnityEvent thisEvent = null;
//		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent)){
//			thisEvent.RemoveListener (listener);
//		}
//	}
//
//	public static void TriggerEvent (string eventName){
//		UnityEvent thisEvent = null;
//		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent)){
//			thisEvent.Invoke ();
//		}
//	}
}