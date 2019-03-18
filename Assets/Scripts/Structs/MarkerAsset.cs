using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MarkerAsset {

    public GENDER gender;
    public Sprite defaultSprite;
    public Sprite hoverSprite;
    public Sprite clickedSprite;
    public RuntimeAnimatorController animator;

    public MarkerAsset(GENDER gender) {
        this.gender = gender;
    }
}
