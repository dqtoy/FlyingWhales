
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ActionIconCharacterInfoUI : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    private CharacterAction _action;
    private Character _character;

    [SerializeField] private Image progressBarImage;
    [SerializeField] private TextMeshProUGUI actionText;

    private int _currentDay;

    #region getters/setters
    public CharacterAction action {
        get { return _action; }
    }
    private int currentDay {
        get { return (_character == null ? _currentDay : _character.currentParty.currentDay); }
    }
    #endregion

    public void Initialize() {
        Messenger.AddListener<CharacterAction, Party>(Signals.ACTION_DAY_ADJUSTED, OnActionDayAdjusted);
        Messenger.AddListener<CharacterAction, Party>(Signals.ACTION_TAKEN, OnActionTaken);
        //Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
    }
    public void SetCharacter(Character character) {
        _character = character;
    }
    public void SetAction(CharacterAction action) {
        _action = action;
        if (_action != null) {
            actionText.text = _action.actionData.actionName;
            UpdateProgress();
        } else {
            actionText.text = "None";
        }
    }
    public void SetCurrentDay(int amount) {
        _currentDay = amount;
    }
    public void UpdateProgress() {
        if (_action == null) {
            return;
        }
        if (_action.actionData.duration == 0) {
            progressBarImage.gameObject.SetActive(false);
        } else {
            progressBarImage.gameObject.SetActive(true);
            progressBarImage.fillAmount = (float)currentDay / (float)_action.actionData.duration;
        }
    }
    private void OnActionDayAdjusted(CharacterAction action, Party party) {
        if (_action == null || _character == null) {
            return;
        }
        if (_action == action && party.characters.Contains(_character)) {
            UpdateProgress();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        //isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        //isHovering = false;
        //UIManager.Instance.HideSmallInfo();
    }

    public void SetAlpha(float alpha) {
        Color color = progressBarImage.color;
        color.a = alpha;
        progressBarImage.color = color;

        //color = middleCircleImage.color;
        //color.a = alpha;
        //middleCircleImage.color = color;

        //color = iconImage.color;
        //color.a = alpha;
        //iconImage.color = color;
    }

    private void OnActionTaken(CharacterAction action, Party party) {
        if (party != null && _character != null) {
            if (party.id == _character.currentParty.id) {
                SetAction(action);
            }
        }
    }
    public override void Reset() {
        base.Reset();
        if (Messenger.eventTable.ContainsKey(Signals.ACTION_DAY_ADJUSTED)) {
            Messenger.RemoveListener<CharacterAction, Party>(Signals.ACTION_DAY_ADJUSTED, OnActionDayAdjusted);
        }
        if (Messenger.eventTable.ContainsKey(Signals.ACTION_TAKEN)) {
            Messenger.RemoveListener<CharacterAction, Party>(Signals.ACTION_TAKEN, OnActionTaken);
        }
        //Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        _action = null;
        _character = null;
        SetAlpha(255f/255f);
        //isHovering = false;
    }

    //private void Update() {
    //    if (isHovering) {
    //        if (_action != null) {
    //            string summary = _action.actionData.actionName;
    //            if (_action.actionData.duration != 0) {
    //                summary += " " + currentDay.ToString() + "/" + _action.actionData.duration.ToString();
    //            }
    //            UIManager.Instance.ShowSmallInfo(summary);
    //        } else {
    //            UIManager.Instance.ShowSmallInfo("NONE");
    //        }
    //    }
    //}
}
