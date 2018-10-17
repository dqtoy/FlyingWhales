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
    private ActionOption[] _actionOptions;
    private Minion _assignedMinion;

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
        _actionOptions = new ActionOption[4];
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
    public void EndResult() {
        if(_endEffect != null) {
            _endEffect();
        }
        interaction.EndInteraction();
    }
}
