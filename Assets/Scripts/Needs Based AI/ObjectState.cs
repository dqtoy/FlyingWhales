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
    [SerializeField] protected ActionEvent _everydayAction;
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
        _actions = new List<CharacterAction>();
    }

    public ObjectState Clone(IObject obj) {
        ObjectState clonedState = new ObjectState(obj);
        clonedState._stateName = this.stateName;
        clonedState._everydayAction = this._everydayAction;
        clonedState._actions = new List<CharacterAction>();
        for (int i = 0; i < this.actions.Count; i++) {
            CharacterAction ogAction = this.actions[i];
            clonedState._actions.Add(ogAction.Clone());
        }
        return clonedState;
    }

    public void SetName(string name) {
        _stateName = name;
    }

    #region Utilities
    public void OnStartState() {
        Messenger.AddListener(Signals.DAY_ENDED, EveryTickEffect);
        _object.StartState(this);
    }
    public void OnEndState() {
        Messenger.RemoveListener(Signals.DAY_ENDED, EveryTickEffect);
        _object.EndState(this);
    }
    public void EveryTickEffect() {
        if (_everydayAction != null) {
            _everydayAction.Invoke(_object);
        }
    }
    public void OnInteractWith(Character character) { }
    public void SetObject(IObject iobject) {
        _object = iobject;
    }
    public CharacterAction GetActionInState(CharacterAction action) {
        for (int i = 0; i < _actions.Count; i++) {
            CharacterAction currentAction = _actions[i];
            if (currentAction.actionType == action.actionType) {
                return currentAction;
            }
        }
        return null;
    }
    #endregion

    #region Actions
    public void AddNewAction(CharacterAction action) {
        _actions.Add(action);
    }
    public void RemoveAction(CharacterAction action) {
        _actions.Remove(action);
    }
    public void SetActions(List<CharacterAction> actions) {
        _actions = actions;
    }
    public CharacterAction GetAction(ACTION_TYPE type) {
        for (int i = 0; i < _actions.Count; i++) {
            CharacterAction currAction = _actions[i];
            if (currAction.actionType == type) {
                return currAction;
            }
        }
        Debug.LogWarning(_object.objectName + " has no action type " + type.ToString() + " in it's current state " + stateName);
        return null;
    }
    #endregion
}
