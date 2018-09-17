using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueItem {

    public CharacterAction action;
    public IObject targetObject;
    public Quest associatedQuest;
    public GameEvent associatedEvent;

    public ActionQueueItem(CharacterAction action, IObject targetObject, Quest associatedQuest = null, GameEvent associatedEvent = null) {
        this.action = action;
        this.targetObject = targetObject;
        this.associatedQuest = associatedQuest;
        this.associatedEvent = associatedEvent;
    }

    public override string ToString() {
        string summary = string.Empty;
        if (action != null) {
            summary += "A: " + action.actionData.actionName;
        } else {
            summary += "A: Null";
        }

        if (targetObject != null) {
            summary += " O: " + targetObject.objectName;
        } else {
            summary += " O: Null";
        }

        if (associatedQuest != null) {
            summary += " Q: " + associatedQuest.name;
        } else {
            summary += " Q: Null";
        }

        if (associatedEvent != null) {
            summary += " E: " + associatedEvent.name;
        } else {
            summary += " E: Null";
        }

        return summary;
    }
}
