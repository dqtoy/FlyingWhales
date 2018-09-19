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

    public GameEvent AddNewEvent(GAME_EVENT eventType) {
        GameEvent gameEvent = CreateNewEvent(eventType);
        _activeEvents.Add(gameEvent);
        return gameEvent;
    }
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
    private GameEvent CreateNewEvent(GAME_EVENT eventType) {
        switch (eventType) {
            case GAME_EVENT.SECRET_MEETING:
                return new SecretMeeting();
            case GAME_EVENT.MONSTER_ATTACK:
                return new MonsterAttackEvent();
            case GAME_EVENT.TEST_EVENT:
                return new TestEvent();
            case GAME_EVENT.DRAGON_ATTACK:
                return new DragonAttack();
            case GAME_EVENT.SUICIDE:
                return new SuicideEvent();
        }
        return null;
    }

    public void GenerateEventForTesting() {
        return;
        if (CharacterManager.Instance.allCharacters.Count > 1) {
            SecretMeeting secretMeetingEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.SECRET_MEETING) as SecretMeeting;
            secretMeetingEvent.Initialize(CharacterManager.Instance.allCharacters[0], CharacterManager.Instance.allCharacters[1]);
        }
    }
}
