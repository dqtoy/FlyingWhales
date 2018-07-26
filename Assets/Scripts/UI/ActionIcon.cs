using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionIcon : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    private CharacterAction _action;
    private ECS.Character _character;

    [SerializeField] private Image progressBarImage;
    [SerializeField] private Image middleCircleImage;
    [SerializeField] private Image iconImage;

    #region getters/setters
    public CharacterAction action {
        get { return _action; }
    }
    #endregion

    public void Initialize() {
        Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_DAY_ADJUSTED, OnActionDayAdjusted);
    }
    public void SetCharacter(ECS.Character character) {
        _character = character;
    }
    public void SetAction(CharacterAction action) {
        _action = action;
        UpdateProgress();
    }

    public void UpdateProgress() {
        if (_character == null || _action == null) {
            return;
        }
        if (_action.actionData.duration == 0) {
            progressBarImage.fillAmount = 1f;
        } else {
            progressBarImage.fillAmount = (float)(_character.currentParty as CharacterParty).actionData.currentDay / (float)_action.actionData.duration;
        }
    }
    private void OnActionDayAdjusted(CharacterAction action, CharacterParty party) {
        if (_action == null || _character == null) {
            return;
        }
        if (_action == action && party.icharacters.Contains(_character)) {
            UpdateProgress();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (_action != null) {
            UIManager.Instance.ShowSmallInfo(_action.actionData.actionName + " " + (_character.currentParty as CharacterParty).actionData.currentDay.ToString() + "/" + _action.actionData.duration.ToString());
        } else {
            UIManager.Instance.ShowSmallInfo("NONE");
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.Instance.HideSmallInfo();
    }

    public void SetAlpha(float alpha) {
        Color color = progressBarImage.color;
        color.a = alpha;
        progressBarImage.color = color;
    }

    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<CharacterAction, CharacterParty>(Signals.ACTION_DAY_ADJUSTED, OnActionDayAdjusted);
        _action = null;
        _character = null;
        SetAlpha(255f/255f);
    }
}
