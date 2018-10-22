using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Linq;
using System;

public class InteractionItem : MonoBehaviour {
    private Interaction _interaction;
    private ActionOption _currentSelectedActionOption;
    private Toggle _toggle;

    public CharacterPortrait portrait;
    public TextMeshProUGUI descriptionText;
    public ActionOptionButton[] actionOptionButtons;
    public Button confirmNoMinionButton;
    public Button confirmMinionButton;
    public GameObject confirmMinionGO;

    private bool _isToggled;

    #region getters/setters
    public Interaction interaction {
        get { return _interaction; }
    }
    #endregion

    private void Start() {
        Messenger.AddListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.AddListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
    }
    private void OnDestroy() {
        Messenger.RemoveListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.RemoveListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
        GameObject.Destroy(_toggle.gameObject);
    }
    private void OnUpdatedInteractionState(Interaction interaction) {
        if(_interaction != null && _interaction == interaction) {
            UpdateState();
        }
    }
    private void OnChangedActivatedState(Interaction interaction) {
        if (_interaction != null && _interaction == interaction) {
            ChangedActivatedState();
        }
    }
    public void SetInteraction(Interaction interaction) {
        _interaction = interaction;
        if(_interaction != null) {
            InitializeActionButtons();
            UpdateState();
            ChangedActivatedState();
        }
    }
    private void InitializeActionButtons() {
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            actionOptionButtons[i].Initialize();
        }
    }
    public void UpdateState() {
        descriptionText.text = _interaction.currentState.description;
        confirmNoMinionButton.gameObject.SetActive(false);
        confirmMinionGO.SetActive(false);
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            if (_interaction.currentState.actionOptions[i] != null) {
                actionOptionButtons[i].SetOption(_interaction.currentState.actionOptions[i]);
                actionOptionButtons[i].gameObject.SetActive(true);
            } else {
                actionOptionButtons[i].gameObject.SetActive(false);
            }
        }
        if (_interaction.isActivated && !_interaction.currentState.isEnd) {
            ActionOption actionOption = _interaction.currentState.chosenOption;
            if (actionOption != null) {
                if (actionOption.assignedMinion != null) {
                    portrait.GeneratePortrait(actionOption.assignedMinion.icharacter.portraitSettings, 95, true);
                } else {
                    portrait.GeneratePortrait(null, 95, true);
                }
                if (actionOption.needsMinion) {
                    confirmMinionGO.SetActive(true);
                } else {
                    confirmNoMinionButton.gameObject.SetActive(true);
                }
            }
        } else {
            portrait.GeneratePortrait(null, 95, true);
            if (_interaction.currentState.isEnd) {
                confirmNoMinionButton.gameObject.SetActive(true);
            }
        }
    }
    private void ChangedActivatedState() {
        ChangeStateAllButtons(!_interaction.isActivated);
        if (_interaction.isActivated) {
            _toggle.targetGraphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleInactiveUnselected;
            _toggle.graphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleInactiveSelected;
        } else {
            _toggle.targetGraphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleActiveUnselected;
            _toggle.graphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleActiveSelected;
        }
    }
    public void SetCurrentSelectedActionOption(ActionOption actionOption) {
        _currentSelectedActionOption = actionOption;
        if (_currentSelectedActionOption.needsMinion) {
            portrait.GeneratePortrait(null, 95, true);
            confirmMinionGO.SetActive(true);
            confirmMinionButton.interactable = false;
            confirmNoMinionButton.gameObject.SetActive(false);
        } else {
            confirmMinionGO.SetActive(false);
            confirmNoMinionButton.gameObject.SetActive(true);
        }
    }
    public void OnClickConfirm() {
        if (!_interaction.currentState.isEnd) {
            _currentSelectedActionOption.ActivateOption(_interaction.interactable);
        } else {
            _interaction.currentState.EndResult();
        }
    }
    private void ChangeStateAllButtons(bool state) {
        confirmNoMinionButton.interactable = state;
        confirmMinionButton.interactable = state;
        if (!state) {
            for (int i = 0; i < actionOptionButtons.Length; i++) {
                actionOptionButtons[i].button.interactable = state;
            }
        }
    }
    public void OnMinionDrop(Transform transform) {
        if (_interaction.isActivated) {
            return;
        }
        MinionItem minionItem = transform.GetComponent<MinionItem>();
        _currentSelectedActionOption.assignedMinion = minionItem.minion;
        portrait.GeneratePortrait(minionItem.portrait.portraitSettings, 95, true);
        if (_currentSelectedActionOption.assignedMinion != null) {
            confirmMinionButton.interactable = true;
        }
    }
    public void SetToggle(Toggle toggle) {
        _toggle = toggle;
        _toggle.onValueChanged.AddListener(OnToggle);
    }
    private void OnToggle(bool state) {
        if(_isToggled != state) {
            _isToggled = state;
            _toggle.targetGraphic.enabled = !state;
            _toggle.interactable = !state;
            if (state) {
                GoToThisPage();
            }
        } 
    }
    private void GoToThisPage() {
        int index = InteractionUI.Instance.GetIndexOfInteraction(this);
        if (InteractionUI.Instance.scrollSnap.CurrentPage != index) {
            InteractionUI.Instance.scrollSnap.ChangePage(index);
        }
    }
}
