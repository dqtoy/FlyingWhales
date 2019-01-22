
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public virtual bool isDone {
        get { return _isDone; }
    }
    public GAME_EVENT type {
        get { return _type; }
    }
    #endregion

    public GameEvent(GAME_EVENT type) {
        id = Utilities.SetID(this);
        _type = type;
        _isDone = false;
        eventActions = new Dictionary<Character, Queue<EventAction>>();
        SetName(Utilities.NormalizeStringUpperCaseFirstLetters(_type.ToString()));
    }

    #region Utilities
    public void SetName(string name) {
        _name = name;
    }
    public void SetPhase(EVENT_PHASE phase) {
        _phase = phase;
    }
    public GameEvent GetBase() {
        return this;
    }
    #endregion

    #region Virtuals
    public virtual void Initialize(List<Character> characters) {
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            eventActions.Add(currCharacter, new Queue<EventAction>());
        }
    }
    public virtual EventAction GetNextEventAction(Character character) {
        if (eventActions[character].Count != 0) {
            return eventActions[character].Dequeue();
        }
        return null;
    }
    public virtual EventAction PeekNextEventAction(Character character) {
        if (eventActions.ContainsKey(character) && eventActions[character].Count != 0) {
            return eventActions[character].Peek();
        }
        return null;
    }
    /*
     Roughly, how long will this event take to complete?
         */
    public virtual int GetEventDurationRoughEstimateInTicks() {
        int longestDuartion = 0;
        foreach (KeyValuePair<Character, Queue<EventAction>> kvp in eventActions) {
            int currCharactersDuration = 0;
            for (int i = 0; i < kvp.Value.Count; i++) {
                EventAction currentAction = kvp.Value.ElementAt(i);
                currCharactersDuration += currentAction.duration;
            }
            if (longestDuartion < currCharactersDuration) {
                longestDuartion = currCharactersDuration;
            }
        }
        return longestDuartion;
    }
    public virtual void EndEvent() {
        _isDone = true;
        EventManager.Instance.RemoveEvent(this);
    }
    public virtual void EndEventForCharacter(Character character) {
        //character.eventSchedule.RemoveElement(this);
    }
    public virtual bool MeetsRequirements(Character character) {
        return true;
    }
    #endregion
}
