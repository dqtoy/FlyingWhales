﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionBtn : MonoBehaviour {
    [SerializeField] private Text _buttonText;
    private CharacterAction _action;

    public void SetAction(CharacterAction action) {
        _action = action;
        _buttonText.text = action.actionType.ToString();
    }

    public void OnClickAction() {
        _action.OnChooseAction(PlayerManager.Instance.playerCharacter.party, UIManager.Instance.playerActionsUI.currentlyShowingLandmark.landmarkObj);
        UIManager.Instance.playerActionsUI.CloseMenu();
    }
}
