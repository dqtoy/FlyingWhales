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

    [Header("Idle")]
    public AnimationClip idle;
    public AnimationClip walk;
    public AnimationClip sleepGround;
    public AnimationClip dead;
    public AnimationClip raiseDead;

    [Header("Attacks")]
    public AnimationClip slashClip;
    public AnimationClip magicClip;
    public AnimationClip arrowClip;
    public AnimationClip biteClip;

    public MarkerAsset(GENDER gender) {
        this.gender = gender;
    }
}
