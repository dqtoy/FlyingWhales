using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestAction {
    public CharacterAction action;
    public IObject targetObject;
    public float requiredPower;

    public QuestAction(CharacterAction action, IObject targetObject, float requiredPower = 0) {
        this.action = action;
        this.targetObject = targetObject;
        this.requiredPower = requiredPower;
    }
}
