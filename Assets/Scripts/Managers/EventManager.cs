using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public static EventManager Instance;

    private List<GameEvent> _activeEvents;
    private List<GameEvent> _pastEvents;

    #region getters/setters
    public List<GameEvent> activeEvents {
        get { return _activeEvents; }
    }
    public List<GameEvent> pastEvents {
        get { return _pastEvents; }
    }
    #endregion

    void Awake () {
        Instance = this;
	}
    void Start() {
        _activeEvents = new List<GameEvent>();
        _pastEvents = new List<GameEvent>();
    }

    public void Initialize() {
        GeneratePartyEvent();
    }

    //public GameEvent AddNewEvent(GAME_EVENT eventType) {
    //    GameEvent gameEvent = CreateNewEvent(eventType);
    //    _activeEvents.Add(gameEvent);
    //    return gameEvent;
    //}
    public void RemoveEvent(GameEvent gameEvent){
        if (_activeEvents.Remove(gameEvent)) {
            AddToPastEvents(gameEvent);
        }
    }
    private void AddToPastEvents(GameEvent gameEvent) {
        _pastEvents.Add(gameEvent);
        if(_pastEvents.Count > 20) {
            _pastEvents.RemoveAt(0);
        }
    }
    //private GameEvent CreateNewEvent(GAME_EVENT eventType) {
    //    switch (eventType) {
    //        case GAME_EVENT.SECRET_MEETING:
    //            return new SecretMeeting();
    //        case GAME_EVENT.MONSTER_ATTACK:
    //            return new MonsterAttackEvent();
    //        case GAME_EVENT.TEST_EVENT:
    //            return new TestEvent();
    //        case GAME_EVENT.DRAGON_ATTACK:
    //            return new DragonAttack();
    //        case GAME_EVENT.SUICIDE:
    //            return new SuicideEvent();
    //        case GAME_EVENT.RESEARCH_SCROLLS:
    //            return new ResearchScrollsEvent();
    //        case GAME_EVENT.PARTY_EVENT:
    //            return new PartyEvent();
    //    }
    //    return null;
    //}

    #region Random Events
    private void GeneratePartyEvent() {
        //List<BaseLandmark> choices = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.INN);
        //if (choices.Count > 0) {
        //    BaseLandmark chosenLandmark = choices[Random.Range(0, choices.Count)];
        //    PartyEvent partyEvent = AddNewEvent(GAME_EVENT.PARTY_EVENT) as PartyEvent;
        //    partyEvent.SetLocation(chosenLandmark);
        //    partyEvent.Initialize();
        //    chosenLandmark.AddAdvertisedEvent(partyEvent);

        //    GameDate endDate = GameManager.Instance.Today();
        //    endDate.AddHours(partyEvent.GetEventDurationRoughEstimateInTicks());

        //    //reschedule making party event 1 - 3 days from the end of this party event
        //    int randomDays = Random.Range(1, 4);
        //    endDate.AddDays(randomDays);
        //    SchedulingManager.Instance.AddEntry(endDate, () => GeneratePartyEvent());
        //}
    }
    #endregion
}
