using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueItem {

    public CharacterAction action;
    public IObject targetObject;

    public ActionQueueItem(CharacterAction action, IObject targetObject) {
        this.action = action;
        this.targetObject = targetObject;
    }
}
