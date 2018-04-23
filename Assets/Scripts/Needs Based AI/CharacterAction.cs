using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class CharacterAction {
    protected ACTION_TYPE _actionType;
    protected string _actionName;
    protected ObjectState _state;
    protected ActionFilter[] _filters;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionType; }
    }
    public ObjectState state {
        get { return _state; }
    }
    #endregion

    public CharacterAction(ObjectState state, ACTION_TYPE actionType) {
        _state = state;
        _actionType = actionType;
    }

    #region Virtuals
    public virtual void PerformAction(Character character) {
    }
    #endregion

    public Reward GetReward() {
        return _state.actionRewardDictionary[this];
    }
}
