using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacterActionAdvertisement {
    public CharacterAction action;
    public int advertisement;

    public void Reset() {
        this.action = null;
        this.advertisement = 0;
    }

    public void Set(CharacterAction action, int advertisement) {
        this.action = action;
        this.advertisement = advertisement;
    }
}
