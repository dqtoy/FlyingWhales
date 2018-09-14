using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ActionAndTarget {
    public CharacterAction action;
    public IObject target;

    public override string ToString() {
        return action.actionData.actionName + "(" + target.objectName + ")";
    }
}
