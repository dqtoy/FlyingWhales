using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

[System.Serializable]
public class CharacterAction {
    //[SerializeField] protected ACTION_TYPE _actionType;
    //[SerializeField] protected string _actionName;
    protected ObjectState _state;
    protected ActionFilter[] _filters;
    public CharacterActionData actionData;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return actionData.actionType; }
    }
    public ObjectState state {
        get { return _state; }
    }
    #endregion

    public CharacterAction(ObjectState state, ACTION_TYPE actionType) {
        _state = state;
        //_actionType = actionType;
    }

    #region Virtuals
    public virtual void PerformAction(Character character) {
    }
    #endregion

    //public Reward GetReward() {
    //    return _state.actions[this];
    //}
}
