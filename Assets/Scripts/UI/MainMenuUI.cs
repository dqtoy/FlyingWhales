using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private EasyTween buttonsTween;
    [SerializeField] private EasyTween titleTween;

    public void Start() {
        buttonsTween.OnValueChangedAnimation(true);
        titleTween.OnValueChangedAnimation(true);
    }

    public void ExitGame() {
        //TODO: Add Confirmation Prompt
        Application.Quit();
    }
}
