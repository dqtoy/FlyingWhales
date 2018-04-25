using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.Linq;
using UnityEngine.Events;

[System.Serializable]
public class ObjectState {
    protected IObject _object;
    [SerializeField] protected string _stateName;
    [SerializeField] protected UnityEvent _everydayAction;
    [SerializeField] protected List<CharacterAction> _actions;

    #region getters/setters
    public IObject obj {
        get { return _object; }
    }
    public string stateName {
        get { return _stateName; }
    }
    public List<CharacterAction> actions {
        get { return _actions; }
    }
    #endregion

    public ObjectState(IObject iobject) {
        _object = iobject;
    }

    public ObjectState Clone(IObject obj) {
        ObjectState clone = new ObjectState(obj);
        clone._stateName = this.stateName;
        clone._actions = new List<CharacterAction>();
        for (int i = 0; i < this.actions.Count; i++) {
            CharacterAction ogAction = this.actions[i];
            clone._actions.Add(ogAction.Clone());
        }
        return clone;
    }

    #region Virtuals
    public virtual void OnStartState() {
        Messenger.AddListener("OnDayEnd", EverydayEffect);
    }
    public virtual void OnEndState() {
        Messenger.RemoveListener("OnDayEnd", EverydayEffect);
    }
    public virtual void EverydayEffect() {
        if(_everydayAction != null) {
            _everydayAction.Invoke();
        }
    }
    public virtual void OnInteractWith(Character character) { }
    #endregion

    #region Utilities
    public void SetObject(IObject iobject) {
        _object = iobject;
    }
    public bool HasActionInState(CharacterAction action, bool changeActionIfTrue = false) {
        for (int i = 0; i < _actions.Count; i++) {
            CharacterAction currentAction = _actions[i];
            if (currentAction.actionType == action.actionType && currentAction.state.obj == action.state.obj) {
                if (changeActionIfTrue) {
                    //TODO: Change action of character to currentAction if it matches
                }
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Actions
    public void AddNewAction(CharacterAction action) {
        _actions.Add(action);
    }
    public void RemoveAction(CharacterAction action) {
        _actions.Remove(action);
    }
    public CharacterAction GetAction(ACTION_TYPE type) {
        for (int i = 0; i < _actions.Count; i++) {
            CharacterAction currAction = _actions[i];
            if (currAction.actionType == type) {
                return currAction;
            }
        }
        return null;
    }
    #endregion
}
