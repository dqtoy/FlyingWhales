using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterDialogChoice {

    public string buttonTitle;
    public UnityAction onClickAction;

    public CharacterDialogChoice(string buttonTitle, UnityAction onClickAction) {
        this.buttonTitle = buttonTitle;
        this.onClickAction = onClickAction;
    }
}
