using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerJobActionButton : MonoBehaviour {

    private Action clickAction;

    public PlayerJobAction action { get; private set; }

    [SerializeField] private Button btn;
    [SerializeField] private TextMeshProUGUI btnLbl;
    [SerializeField] private TextMeshProUGUI subTextLbl;
    [SerializeField] private Image actionIcon;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private GameObject cover;
    //[SerializeField] private GameObject pointer;
    private Character character;
    private object target;

    private void OnEnable() {
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobActionCooldownActivated);
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobActionCooldownDone);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
    }
    private void OnDisable() {
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobActionCooldownActivated);
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobActionCooldownDone);
        Messenger.RemoveListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.RemoveListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
    }

    public void SetJobAction(PlayerJobAction action, Character character) {
        this.action = action;
        this.character = character;
        actionIcon.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
        UpdateInteractableState();
        UpdateButtonText();
        SetSelectedIconState(false);
    }

    //private void OnSeenActionButtons() {
    //    pointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    //}

    #region Visuals
    public void UpdateInteractableState() {
        SetInteractableState(
            !action.parentData.hasActionInCooldown 
            && InteriorMapManager.Instance.isAnAreaMapShowing 
            && PlayerManager.Instance.player.currentActivePlayerJobAction != this.action
        );
    }
    private void SetInteractableState(bool state) {
        btn.interactable = state;
        cover.SetActive(!state);
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
    public void SetSelectedIconState(bool state) {
        selectedIcon.gameObject.SetActive(state);
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
            message = "The Instigator will rile up a character and goad him into attacking people in a specified location. This action only works for beasts.";
            header += "(Instigator Action)";
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
        } else if (action is AccessMemories) {
            message = "Access the memories of a character.";
            header += "(Spy Action)";
        } else if (action is Abduct) {
            message = "The Instigator will goad a character into abducting a specified character. This action only works on goblins and skeletons.";
            header += "(Instigator Action)";
        } else if (action is Zap) {
            message = "Temporarily prevents a character from moving for 30 minutes.";
            header += "(Debilitator Action)";
        } else if (action is Jolt) {
            message = "Temporarily speeds up the movement of a character.";
            header += "(Debilitator Action)";
        } else if (action is Spook) {
            message = "Temporarily forces a character to flee from all other nearby characters.";
            header += "(Debilitator Action)";
        } else if (action is Enrage) {
            message = "Temporarily enrages a character.";
            header += "(Debilitator Action)";
        } else if (action is CorruptLycanthropy) {
            message = "Inflict a character with Lycanthropy, which gives a character a chance to transform into a wild wolf whenever he/she sleeps.";
            header += "(Seducer Action)";
        } else if (action is CorruptKleptomaniac) {
            message = "Inflict a character with Kleptomania, which will make that character enjoy stealing other people's items.";
            header += "(Seducer Action)";
        } else if (action is CorruptVampiric) {
            message = "Inflict a character with Vampirism, which will make that character need blood for sustenance.";
            header += "(Seducer Action)";
        } else if (action is CorruptUnfaithful) {
            message = "Make a character prone to have affairs.";
            header += "(Seducer Action)";
        } else if (action is RaiseDead) {
            message = "Return a character to life.";
            header += "(Instigator Action)";
        }

        if (action.parentData.hasActionInCooldown) {
            header += " (On Cooldown)";
        }
        PlayerManager.Instance.player.SeenActionButtonsOnce();
        PlayerUI.Instance.ShowActionBtnTooltip(message, header);
    }
    public void HideHoverText() {
        PlayerUI.Instance.HideActionBtnTooltip();
    }
    #endregion

    #region Listeners
    private void OnJobActionCooldownActivated(PlayerJobAction jobAction) {
        if (jobAction.parentData == this.action.parentData) {
            UpdateInteractableState();
        }
    }
    private void OnJobActionCooldownDone(PlayerJobAction jobAction) {
        if (jobAction.parentData == this.action.parentData) {
            UpdateInteractableState();
        }
    }
    private void OnAreaMapOpened(Area area) {
        UpdateInteractableState();
    }
    private void OnAreaMapClosed(Area area) {
        UpdateInteractableState();
    }
    #endregion

}
