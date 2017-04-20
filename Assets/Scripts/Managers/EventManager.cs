using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventManager : MonoBehaviour {
	public static EventManager Instance;

	private Dictionary <string, UnityEvent> eventDictionary;

	public Dictionary<EVENT_TYPES, List<GameEvent>> allEvents;

	public WeekEndedEvent onWeekEnd = new WeekEndedEvent();
	public NewKingdomEvent onCreateNewKingdomEvent = new NewKingdomEvent();
	public CitizenTurnActions onCitizenTurnActions = new CitizenTurnActions ();
	public CityEverydayTurnActions onCityEverydayTurnActions = new CityEverydayTurnActions();
	public CitizenMove onCitizenMove =  new CitizenMove();
	public CitizenDiedEvent onCitizenDiedEvent =  new CitizenDiedEvent();
	public RegisterOnCampaign onRegisterOnCampaign = new RegisterOnCampaign();
	public DeathArmy onDeathArmy = new DeathArmy();
	public UnsupportCitizen onUnsupportCitizen = new UnsupportCitizen();
	public GameEventAction onGameEventAction = new GameEventAction();
	public CheckCitizensSupportingMe onCheckCitizensSupportingMe = new CheckCitizensSupportingMe();
	public RecruitCitizensForExpansion onRecruitCitizensForExpansion = new RecruitCitizensForExpansion();
	public GameEventEnded onGameEventEnded = new GameEventEnded();
	public ShowEventsOfType onShowEventsOfType = new ShowEventsOfType();
	public HideEvents onHideEvents = new HideEvents();
	public RemoveSuccessionWarCity onRemoveSuccessionWarCity = new RemoveSuccessionWarCity();
	public UpdateUI onUpdateUI = new UpdateUI();

	public EVENT_TYPES eventTypeForTesting;

	[ContextMenu("Add Event")]
	public void AddEvent(){
		if (eventTypeForTesting == EVENT_TYPES.MARRIAGE_INVITATION) {
			MarriageInvitation eventToCreate = new MarriageInvitation(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, null);
			this.AddEventToDictionary(eventToCreate);
		}
	}

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

	public void AddEventToDictionary(GameEvent gameEvent){
		if (allEvents.ContainsKey (gameEvent.eventType)) {
			allEvents [gameEvent.eventType].Add(gameEvent);
		} else {
			allEvents.Add (gameEvent.eventType, new List<GameEvent> (){ gameEvent });
		}
	}
	public List<GameEvent> GetEventsOfType(EVENT_TYPES eventType){
		List<GameEvent> eventsOfType = new List<GameEvent>();
		if (this.allEvents.ContainsKey (eventType)) {
			eventsOfType = this.allEvents[eventType];
		}
		return eventsOfType;
	}

	public List<GameEvent> GetEventsOfTypePerKingdom(Kingdom kingdom, EVENT_TYPES eventType){
		List<GameEvent> gameEventsOfTypePerKingdom = new List<GameEvent>();
		if (this.allEvents.ContainsKey (eventType)) {
			List<GameEvent> eventsOfType = this.allEvents[eventType];
			for (int i = 0; i < eventsOfType.Count; i++) {
				if (eventsOfType[i].startedByKingdom.id == kingdom.id) {
					gameEventsOfTypePerKingdom.Add(eventsOfType[i]);
				}
			}
		}
		return gameEventsOfTypePerKingdom;
	}

	public List<GameEvent> GetAllEventsPerCity(City city){
		List<GameEvent> gameEventsOfCity = new List<GameEvent>();
		for (int i = 0; i < this.allEvents.Keys.Count; i++) {
			EVENT_TYPES currentKey = this.allEvents.Keys.ElementAt(i);
			List<GameEvent> gameEventsOfType = this.allEvents [currentKey];
			for (int j = 0; j < gameEventsOfType.Count; j++) {
				if (gameEventsOfType[j].startedByCity.id == city.id) {
					gameEventsOfCity.Add(gameEventsOfType [j]);
				}
			}
		}
		return gameEventsOfCity;
	}

	internal Citizen GetSpy(Kingdom kingdom){
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
			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}

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