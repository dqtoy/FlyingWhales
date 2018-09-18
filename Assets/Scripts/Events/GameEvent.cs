using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent {
    public int id { get; private set; }
    protected string _name;
    protected GAME_EVENT _type;
    protected EVENT_PHASE _phase;
    protected bool _isDone;

    protected Dictionary<Character, Queue<EventAction>> eventActions;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public bool isDone {
        get { return _isDone; }
    }
    #endregion

    public GameEvent(GAME_EVENT type) {
        id = Utilities.SetID(this);
        _type = type;
        _isDone = false;
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
        if (eventActions[character].Count != 0) {
            return eventActions[character].Dequeue();
        }
        return null;
    }
    public virtual EventAction PeekNextEventAction(Character character) {
        if (eventActions[character].Count != 0) {
            return eventActions[character].Peek();
        }
        return null;
    }
    /*
     Roughly, how long will this event take to complete?
         */
    public virtual int GetEventDurationRoughEstimateInTicks() {
        return 0;
    }
    public virtual void EndEvent() {
        _isDone = true;
        EventManager.Instance.RemoveEvent(this);
    }
    public virtual void EndEventForCharacter(Character character) {
        character.eventSchedule.RemoveElement(this);
    }
    #endregion
}
