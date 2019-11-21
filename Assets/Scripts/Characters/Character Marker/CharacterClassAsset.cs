using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterClassAsset {

    [Header("Sprites")]
    public Sprite defaultSprite;

    [Header("Animators")]
    public RuntimeAnimatorController animator;

    [Header("Animations")]
    public AnimationClip idleClip;
    public AnimationClip attackClip;
    public AnimationClip walkClip;
    public AnimationClip deadClip;
    public AnimationClip raiseDeadClip;
    public AnimationClip sleepGroundClip;
}
