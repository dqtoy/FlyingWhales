using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

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
		if (this.allEvents.ContainsKey (eventType)) {
			return this.allEvents[eventType];
		}
		return null;
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