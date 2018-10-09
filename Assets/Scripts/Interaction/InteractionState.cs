using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionState {
    private string _name;
    private string _description;
    private ActionOption[] _actionOptions;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string description {
        get { return _description; }
    }
    public ActionOption[] actionOptions {
        get { return _actionOptions; }
    }
    #endregion

    public InteractionState(string name) {
        _name = name;
        _actionOptions = new ActionOption[4];
    }

    public void SetDescription(string desc) {
        _description = desc;
    }
    public void AddActionOption(ActionOption option) {
        for (int i = 0; i < _actionOptions.Length; i++) {
            if(_actionOptions[i].description == string.Empty) {
                _actionOptions[i] = option;
            }
        }
    }
}
