using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterClassAsset {

    [Header("Sprites")]
    public Sprite defaultSprite;

    [Header("Animation Sprites")]
    public List<Sprite> animationSprites;

    public CharacterClassAsset() {
        animationSprites = new List<Sprite>();
    }
}
