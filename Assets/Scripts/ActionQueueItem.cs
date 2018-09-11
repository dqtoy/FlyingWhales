using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueItem {

    public CharacterAction action;
    public IObject targetObject;
    public Quest associatedQuest;

    public ActionQueueItem(CharacterAction action, IObject targetObject, Quest associatedQuest = null) {
        this.action = action;
        this.targetObject = targetObject;
        this.associatedQuest = associatedQuest;
    }
}
