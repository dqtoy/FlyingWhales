using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAction {
    
    public CharacterAction action { get; private set; }
    public IObject targetObject { get; private set; }
    public int duration { get; private set; }
    public GameEvent associatedEvent { get; private set; }
    public ILocation targetLocation { get { return targetObject.specificLocation; } }

    public EventAction(CharacterAction action, IObject targetObject, GameEvent associatedEvent, int duration = -1) {
        this.action = action;
        this.targetObject = targetObject;
        this.associatedEvent = associatedEvent;
        this.duration = duration;
    }
}
