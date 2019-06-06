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
    [SerializeField] private Image actionIcon;
    [SerializeField] private GameObject cover;
    //[SerializeField] private GameObject pointer;
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

    public void SetJobAction(PlayerJobAction action, Character character) {
        this.action = action;
        this.character = character;
        actionIcon.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
        UpdateInteractableState();
        UpdateButtonText();
    }

    //private void OnSeenActionButtons() {
    //    pointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    //}

    #region Visuals
    private void UpdateInteractableState() {
        SetInteractableState(action.ShouldButtonBeInteractable());
    }
    private void SetInteractableState(bool state) {
        btn.interactable = state;
        cover.SetActive(!state);
    }
    private void UpdateSubText() {
        subTextLbl.text = action.btnSubText;
    }
    private void UpdateButtonText() {
        btnLbl.text = action.name;
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
        string header = action.name + " ";
        if (action is Track) {
            message = "The Spy will keep track of every significant event happening to a target. A notification will be displayed " +
                "for each event related to the target even when the target is not actively selected.";
            header += "(Spy Action)";
        } else if (action is Corrupt) {
            message = "The Seducer will afflict the target with a negative Trait.";
            header += "(Seducer Action)";
        } else if (action is Recruit) {
            message = "The Seducer will attempt to recruit a character to your side. This is only possible while the target is mentally vulnerable.";
            header += "(Seducer Action)";
        } else if (action is ShareIntel) {
            message = "The Diplomat will reach out to a character and share a piece of information with them.";
            header += "(Diplomat Action)";
        } else if (action is RileUp) {
            RileUp rileUp = action as RileUp;
            //if (rileUp.GetActionName(target) == "Abduct") {
            //    message = "The Instigator will goad " + target.name + " into abducting a specified character. This action only works on goblins.";
            //    header = "Instigator Action";
            //} else {
                message = "The Instigator will rile up a character and goad him into attacking people in a specified location. This action only works for beasts.";
                header += "(Instigator Action)";
            //}
        } else if (action is Intervene) {
            message = "The Debilitator will convince a character to drop his current plans.";
            header += "(Debilitator Action)";
        } else if (action is Provoke) {
            message = "The Instigator will provoke a character into attacking one of his/her enemies. This is more likely to succeed if he/she is in a bad mood.";
            header += "(Instigator Action)";
        } else if (action is Destroy) {
            message = "Remove this object from the world.";
            header += "(Instigator Action)";
        } else if (action is Disable) {
            message = "Prevent characters from using this object for 4 hours.";
            header += "(Debilitator Action)";
        }
        
        if (action.isInCooldown) {
            header += " (On Cooldown)";
        }
        PlayerManager.Instance.player.SeenActionButtonsOnce();
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
            //UpdateSubText();
        }
    }
}
