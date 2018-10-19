using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InteractionState {
    private Interaction _interaction;
    private string _name;
    private string _description;
    private bool _isEnd;
    private Action _endEffect;
    private Minion _assignedMinion;
    private ActionOption _chosenOption;
    private ActionOption[] _actionOptions;
    private int _timeLimit;
    private int _defaultActionOptionIndex;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string description {
        get { return _description; }
    }
    public bool isEnd {
        get { return _isEnd; }
    }
    public ActionOption chosenOption {
        get { return _chosenOption; }
    }
    public Interaction interaction {
        get { return _interaction; }
    }
    public Minion assignedMinion {
        get { return _assignedMinion; }
    }
    public ActionOption[] actionOptions {
        get { return _actionOptions; }
    }
    #endregion

    public InteractionState(string name, Interaction interaction) {
        _interaction = interaction;
        _name = name;
        _chosenOption = null;
        _actionOptions = new ActionOption[4];
        _timeLimit = -1;
    }

    public void OnSetAsCurrentState() {
        if (_timeLimit != -1) {

        }
    }
    public void SetDescription(string desc) {
        _description = desc;
    }
    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void AddActionOption(ActionOption option) {
        for (int i = 0; i < _actionOptions.Length; i++) {
            if(_actionOptions[i] == null) {
                _actionOptions[i] = option;
                break;
            }
        }
    }
    public void SetEndEffect(Action endEffect) {
        if (endEffect != null) {
            _isEnd = true;
        } else {
            _isEnd = false;
        }
        _endEffect = endEffect;
    }
    public void SetChosenOption(ActionOption option) {
        _chosenOption = option;
    }
    public void EndResult() {
        if(_endEffect != null) {
            _endEffect();
        }
        if(_assignedMinion != null && !_assignedMinion.icharacter.isDead) {
            _assignedMinion.GoBackFromAssignment();
        }
        interaction.EndInteraction();
    }

    #region Time Limit
    public void SetTimeLimit(int timeLimit, int defaultActionOptionIndex) {
        _timeLimit = timeLimit;
        _defaultActionOptionIndex = defaultActionOptionIndex;
    }
    private void StartTimeLimit() {
        Messenger.AddListener(Signals.HOUR_ENDED, CheckForTimeLimit);
    }
    private void CheckForTimeLimit() {
        if (_timeLimit > 0) {
            _timeLimit -= 1;
        }
        if (_timeLimit == 0) {
            //execute default option
            Messenger.RemoveListener(Signals.HOUR_ENDED, CheckForTimeLimit);
            actionOptions[_defaultActionOptionIndex].ActivateOption(_interaction.interactable);
        }
    }
    #endregion
}
