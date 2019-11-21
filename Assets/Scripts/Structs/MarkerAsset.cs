using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenderMarkerAsset {

    public GENDER gender;
    //public Sprite defaultSprite;
    //public Sprite hoverSprite;
    //public Sprite clickedSprite;
    //public RuntimeAnimatorController animator;

    //[Header("Idle")]
    //public AnimationClip idle;
    //public AnimationClip walk;
    //public AnimationClip sleepGround;
    //public AnimationClip dead;
    //public AnimationClip raiseDead;

    //[Header("Attacks")]
    //public AnimationClip slashClip;
    //public AnimationClip magicClip;
    //public AnimationClip arrowClip;
    //public AnimationClip biteClip;

    //[Header("Attack Timing")]
    //public float slashTiming;
    //public float magicTiming;
    //public float arrowTiming;
    //public float biteTiming;

    public CharacterClassAssetDictionary characterClassAssets;

    public GenderMarkerAsset(GENDER gender) {
        this.gender = gender;
        characterClassAssets = new CharacterClassAssetDictionary();
    }
}
