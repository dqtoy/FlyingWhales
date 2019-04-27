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
        UpdateInteractableState();
        UpdateSubText();
        UpdateButtonText();
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
    private void UpdateButtonText() {
        btnLbl.text = action.actionName;
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

    #region Hover
    public void ShowHoverText() {
        string message = string.Empty;
        string header = string.Empty;
        Character target = this.target as Character;
        if (action is Track) {
            message = "The Spy will keep track of every significant event happening to " + target.name + ". A notification will be displayed " +
                "for each event related to " + target.name + " even when " + Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false) + " is not actively selected.";
            header = "Spy Action.";
        } else if (action is Corrupt) {
            message = "The Seducer will afflict " + target.name + " with a negative Trait.";
            header = "Seducer Action.";
        } else if (action is Recruit) {
            message = "The Seducer will attempt to recruit " + target.name + " to your side. This is only possible while " + target.name + " is mentally vulnerable.";
            header = "Seducer Action.";
        } else if (action is ShareIntel) {
            message = "The Diplomat will reach out to " + target.name + " and share a piece of information with " + Utilities.GetPronounString(target.gender, PRONOUN_TYPE.REFLEXIVE, false) + ".";
            header = "Diplomat Action.";
        } else if (action is RileUp) {
            RileUp rileUp = action as RileUp;
            if (rileUp.GetActionName(target) == "Abduct") {
                message = "The Instigator will goad " + target.name + " into abducting a specified character. This action only works on goblins.";
                header = "Instigator Action.";
            } else {
                message = "The Instigator will rile up " + target.name + " and goad him into attacking people in a specified location. This action only works for beasts.";
                header = "Instigator Action.";
            }
        } else if (action is Intervene) {
            message = "The Debilitator will convince " + target.name + " to drop his current plans.";
            header = "Debilitator Action.";
        }

        PlayerUI.Instance.ShowActionBtnTooltip(message, header);
    }
    public void HideHoverText() {
        PlayerUI.Instance.HideActionBtnTooltip();
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
