using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class CharacterInfoEditor : MonoBehaviour {

    private Character _character;

    [SerializeField] private CharacterPortrait portrait;

    [Header("Portrait Editor")]
    [SerializeField] private Stepper hairStepper;

    #region Monobehaviours
    private void Awake() {
        SetStepperValues();
    }
    #endregion

    public void ShowCharacterInfo(Character character) {
        _character = character;
        portrait.GeneratePortrait(character);
        this.gameObject.SetActive(true);
        UpdateSteppers();
    }
    public void Close() {
        this.gameObject.SetActive(false);
    }

    #region Portrait Editor
    private void SetStepperValues() {
        //Hair
        hairStepper.maximum = CharacterManager.Instance.hairSettings.Count - 1;
    }
    private void UpdateSteppers() {
        hairStepper.value = _character.portraitSettings.hairIndex;
    }
    public void UpdateHair(int hairIndex) {
        portrait.SetHair(hairIndex);
    }
    #endregion
}
