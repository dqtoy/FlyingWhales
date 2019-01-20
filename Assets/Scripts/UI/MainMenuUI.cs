using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private EasyTween buttonsTween;
    [SerializeField] private EasyTween titleTween;

    [SerializeField] private EasyTween glowTween;
    [SerializeField] private EasyTween glow2Tween;

    public void Start() {
        buttonsTween.OnValueChangedAnimation(true);
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
    }

    public void ExitGame() {
        //TODO: Add Confirmation Prompt
        Application.Quit();
    }

    public void Glow2TweenPlayForward() {
        glow2Tween.OnValueChangedAnimation(true);
    }

    public void Glow2TweenPlayReverse() {
        glow2Tween.OnValueChangedAnimation(false);
    }

    public void GlowTweenPlayForward() {
        glowTween.OnValueChangedAnimation(true);
    }

    public void GlowTweenPlayReverse() {
        glowTween.OnValueChangedAnimation(false);
    }


}
