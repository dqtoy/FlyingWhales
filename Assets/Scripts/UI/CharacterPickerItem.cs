using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPickerItem : ObjectPickerItem<Character> {

    public Action<Character> onClickAction;

    private Character character;

    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private GameObject portraitCover;

    public void SetCharacter(Character character) {
        this.character = character;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.GeneratePortrait(character);
        mainLbl.text = character.name;
        subLbl.text = Utilities.GetNormalizedSingularRace(character.race) + " " + character.characterClass.className;
    }

    public void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(character);
        }
    }
}
