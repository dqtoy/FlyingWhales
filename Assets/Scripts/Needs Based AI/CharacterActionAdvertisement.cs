using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacterActionAdvertisement {
    public CharacterAction action;
    public IObject targetObject;
    public float advertisement;

    public void Reset() {
        this.action = null;
        this.targetObject = null;
        this.advertisement = 0;
    }

    public void Set(CharacterAction action, IObject targetObject, float advertisement) {
        this.action = action;
        this.targetObject = targetObject;
        this.advertisement = advertisement;
    }
}
