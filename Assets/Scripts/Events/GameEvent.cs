using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent {
    protected string _name;
    protected GAME_EVENT _type;
    protected EVENT_PHASE _phase;

    protected Dictionary<Character, Queue<EventAction>> eventActions;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    #endregion

    public GameEvent(GAME_EVENT type) {
        _type = type;
        SetName(Utilities.NormalizeStringUpperCaseFirstLetters(_type.ToString()));
    }

    #region Utilities
    public void SetName(string name) {
        _name = name;
    }
    public void SetPhase(EVENT_PHASE phase) {
        _phase = phase;
    }
    #endregion

    #region Virtuals
    public virtual EventAction GetNextEventAction(Character character) {
        return null;
    }
    /*
     Roughly, how long will this event take to complete?
         */
    public virtual int GetEventDurationRoughEstimate() {
        return 0;
    }
    #endregion
}
