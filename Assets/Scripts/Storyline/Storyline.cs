﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storyline {
    protected string _name;
    protected string _description;
    protected STORYLINE _storylineType;
    protected int _lastEventID;
    protected StorylineEvent _activeEvent;
    protected Dictionary<int, StorylineEvent> _events;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public STORYLINE storylineType {
        get { return _storylineType; }
    }
    #endregion

    public Storyline(STORYLINE type) {
        _storylineType = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_storylineType.ToString());
        _events = new Dictionary<int, StorylineEvent>();
        _lastEventID = 0;
    }

    #region Virtuals
    public virtual void ConstructEventTree() {

    }
    public virtual void PopulateStorylineEvents() {

    }
    public virtual void CreateNewStorylineEvent() {
        _lastEventID += 1;
    }
    #endregion

    #region Utilities
    public void StartStory(StorylineEvent startingEvent) {
        _activeEvent = startingEvent;
        _activeEvent.OnStartEvent();
    }
    public void AddStorylineEvent(StorylineEvent storylineEvent) {
        _events.Add(_lastEventID, storylineEvent);
    }
    public void TransitionToNextEvent(int arrayIndex) {
        int idOfNextEvent = _activeEvent.choices[arrayIndex];
        _activeEvent.OnEndEvent();
        _activeEvent = _events[idOfNextEvent];
        _activeEvent.OnStartEvent();
    }
    #endregion
}