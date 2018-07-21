using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueItem {

    public CharacterAction action;
    public IObject targetObject;
    public CharacterQuestData associatedQuestData;

    public ActionQueueItem(CharacterAction action, IObject targetObject, CharacterQuestData associatedQuestData = null) {
        this.action = action;
        this.targetObject = targetObject;
        this.associatedQuestData = associatedQuestData;
    }
}
