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
        string message;
        switch (currentlySelectedSummon) {
            case SUMMON_TYPE.Wolf:
                message = "Summon a wolf to run amok.";
                break;
            case SUMMON_TYPE.Skeleton:
                message = "Summon a skeleton that will abduct a random character.";
                break;
            case SUMMON_TYPE.Golem:
                message = "Summon a stone golem that can sustain alot of hits.";
                break;
            case SUMMON_TYPE.Succubus:
                message = "Summon a succubus that will seduce a male character and eliminate him.";
                break;
            case SUMMON_TYPE.Incubus:
                message = "Summon a succubus that will seduce a female character and eliminate her.";
                break;
            case SUMMON_TYPE.Thief:
                message = "Summon a thief that will steal items from the settlements warehouse.";
                break;
            default:
                message = "Summon a " + Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummon.ToString());
                break;
        }
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
