using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacterActionAdvertisement {
    public CharacterAction action;
    public float advertisement;

    public void Reset() {
        this.action = null;
        this.advertisement = 0;
    }

    public void Set(CharacterAction action, float advertisement) {
        this.action = action;
        this.advertisement = advertisement;
    }
}
