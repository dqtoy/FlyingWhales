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
    //private Character character;
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
        //this.character = character;
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
            //!action.parentData.hasActionInCooldown &&
            InteriorMapManager.Instance.isAnAreaMapShowing 
            && PlayerManager.Instance.player.currentActivePlayerJobAction != this.action && !action.isInCooldown
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
        string header = action.name;
        string message = action.description;
        PlayerUI.Instance.ShowActionBtnTooltip(message, header);
    }
    public void HideHoverText() {
        PlayerUI.Instance.HideActionBtnTooltip();
    }
    #endregion

    #region Listeners
    private void OnJobActionCooldownActivated(PlayerJobAction jobAction) {
        if (jobAction == this.action) {
            UpdateInteractableState();
        }
    }
    private void OnJobActionCooldownDone(PlayerJobAction jobAction) {
        if (jobAction == this.action) {
            UpdateInteractableState();
        }
    }
    private void OnAreaMapOpened(Area area) {
        UpdateInteractableState();
    }
    private void OnAreaMapClosed(Area area) {
        //Upon closing of area map reset intervention ability cooldowns
        //action.InstantCooldown();
        UpdateInteractableState();
    }
    #endregion

}
