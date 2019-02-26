using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerJobActionButton : MonoBehaviour {

    private Action clickAction;

    private PlayerJobAction action;

    [SerializeField] private Button btn;
    [SerializeField] private TextMeshProUGUI btnLbl;
    [SerializeField] private TextMeshProUGUI subTextLbl;

    private Character character;
    private object target;

    private void OnEnable() {
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobActionCooldownActivated);
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobActionCooldownDone);
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_SUB_TEXT_CHANGED, OnSubTextChanged);
    }
    private void OnDisable() {
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobActionCooldownActivated);
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobActionCooldownDone);
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_SUB_TEXT_CHANGED, OnSubTextChanged);
    }

    public void SetJobAction(PlayerJobAction action, Character character, object target) {
        this.action = action;
        this.character = character;
        this.target = target;
        btnLbl.text = action.actionName;
        UpdateInteractableState();
        UpdateSubText();
    }

    #region Visuals
    private void UpdateInteractableState() {
        SetInteractableState(action.ShouldButtonBeInteractable(character, target));
    }
    private void SetInteractableState(bool state) {
        btn.interactable = state;
    }
    private void UpdateSubText() {
        subTextLbl.text = action.btnSubText;
    }
    #endregion

    #region Click
    public void SetClickAction(Action action) {
        clickAction = action;
    }
    public void OnClickAction() {
        if (clickAction != null) {
            clickAction();
            UpdateInteractableState();
        }
    }
    #endregion

    private void OnJobActionCooldownActivated(PlayerJobAction jobAction) {
        if (jobAction == action) {
            UpdateInteractableState();
        }
    }
    private void OnJobActionCooldownDone(PlayerJobAction jobAction) {
        if (jobAction == action) {
            UpdateInteractableState();
        }
    }

    public void OnSubTextChanged(PlayerJobAction jobAction) {
        if (jobAction == action) {
            UpdateSubText();
        }
    }
}
