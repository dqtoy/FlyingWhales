using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LandmarkStructureSprite {
    public Sprite mainSprite;
    public Sprite tintSprite;
    public RuntimeAnimatorController animation;

    public LandmarkStructureSprite(Sprite mainSprite, Sprite tintSprite, RuntimeAnimatorController animation = null) {
        this.mainSprite = mainSprite;
        this.tintSprite = tintSprite;
        this.animation = animation;
    }
}
