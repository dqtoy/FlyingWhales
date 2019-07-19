using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionItem : MonoBehaviour {

    public object obj { get; private set; }

    [SerializeField] protected Image actionIcon;
    [SerializeField] protected CharacterPortrait portraitIcon;
    [SerializeField] protected Button actionBtn;
    [SerializeField] protected UIHoverPosition hoverPos;
    [SerializeField] protected GameObject buttonCover;


    public void SetAction(object obj) {
        this.obj = obj;
        if (obj == null) {
            actionIcon.gameObject.SetActive(false);
            portraitIcon.gameObject.SetActive(false);
        } else if (obj is Summon) {
            Summon summon = obj as Summon;
            actionIcon.gameObject.SetActive(false);
            portraitIcon.gameObject.SetActive(true);
            portraitIcon.GeneratePortrait(summon);
            AddSummonListeners();
        }
    }
    private void ClearAction() {
        if (obj is Summon) {
            RemoveSummonListeners();
        }
        SetAction(null);
    }

    #region Interactions
    public void OnClick() {
        if (obj is Summon) {
            PlayerUI.Instance.SetCurrentlySelectedSummon((obj as Summon).summonType);
            PlayerUI.Instance.OnClickSummon();
        }
    }
    public void OnHoverEnter() {
        if (obj is Summon) {
            ShowSummonTooltip((obj as Summon).summonType);
        }
    }
    public void OnHoverExit() {
        UIManager.Instance.HideSmallInfo();
    }
    private void Disable() {
        actionBtn.interactable = false;
        buttonCover.SetActive(true);
    }
    private void Enable() {
        actionBtn.interactable = true;
        buttonCover.SetActive(false);
    }
    #endregion

    #region Summons
    private void AddSummonListeners() {
        Messenger.AddListener<Summon>(Signals.PLAYER_REMOVED_SUMMON, OnPlayerRemovedSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_PLACED_SUMMON, OnPlayerPlacedSummon);
    }
    private void RemoveSummonListeners() {
        Messenger.RemoveListener<Summon>(Signals.PLAYER_REMOVED_SUMMON, OnPlayerRemovedSummon);
        Messenger.RemoveListener<Summon>(Signals.PLAYER_PLACED_SUMMON, OnPlayerPlacedSummon);
    }
    private void ShowSummonTooltip(SUMMON_TYPE currentlySelectedSummon) {
        string header = Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummon.ToString());
        string message = PlayerManager.Instance.player.GetSummonDescription(currentlySelectedSummon);
        UIManager.Instance.ShowSmallInfo(message, hoverPos, header);
    }
    private void OnPlayerRemovedSummon(Summon summon) {
        if (obj == summon) {
            ClearAction();
        }
    }
    private void OnPlayerPlacedSummon(Summon summon) {
        if (obj == summon) {
            Disable();
        }
    }
    #endregion


}
