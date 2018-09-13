using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAction {
    
    public CharacterAction action { get; private set; }
    public IObject targetObject { get; private set; }

    public EventAction(CharacterAction action, IObject targetObject) {
        this.action = action;
        this.targetObject = targetObject;
    }
}
