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
    [SerializeField] protected CharacterActionData _actionData;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionData.actionType; }
    }
    public ObjectState state {
        get { return _state; }
    }
    #endregion

    public CharacterAction(ObjectState state, ACTION_TYPE actionType) {
        _state = state;
        //_actionType = actionType;
    }

    public CharacterAction Clone(ObjectState state) {
        CharacterAction clone = new CharacterAction(state, this.actionType);
        if (this._filters != null) {
            clone._filters = new ActionFilter[this._filters.Length];
            for (int i = 0; i < this._filters.Length; i++) {
                clone._filters[i] = this._filters[i];
            }
        }
        clone._actionData = this._actionData;
        return clone;
    }

    #region Virtuals
    public virtual void PerformAction(Character character) {
    }
    #endregion

    //public Reward GetReward() {
    //    return _state.actions[this];
    //}
}
