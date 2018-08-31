using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LandmarkStructureSprite {
    public Sprite mainSprite;
    public Sprite tintSprite;

    public LandmarkStructureSprite(Sprite mainSprite, Sprite tintSprite) {
        this.mainSprite = mainSprite;
        this.tintSprite = tintSprite;
    }
}
