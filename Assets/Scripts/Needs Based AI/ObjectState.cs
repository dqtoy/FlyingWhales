using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.Linq;

public class ObjectState {
    protected IObject _object;
    protected string _stateName;
    protected Dictionary<CharacterAction, Reward> _actionRewardDictionary;

    #region getters/setters
    public IObject obj {
        get { return _object; }
    }
    public string stateName {
        get { return _stateName; }
    }
    public float foodAdvertisementMod {
        get { return _foodAdvertisementMod; }
    }
    public float energyAdvertisementMod {
        get { return _energyAdvertisementMod; }
    }
    public float joyAdvertisementMod {
        get { return _joyAdvertisementMod; }
    }
    public float prestigeAdvertisementMod {
        get { return _prestigeAdvertisementMod; }
    }
    public Dictionary<CharacterAction, Reward> actionRewardDictionary {
        get { return _actionRewardDictionary; }
    }
    #endregion

    public ObjectState(IObject obj, string stateName) {
        _object = obj;
        _stateName = stateName;
        _actionRewardDictionary = new Dictionary<CharacterAction, Reward>();
    }

    #region Virtuals
    protected virtual void OnStartState() {
        Messenger.AddListener("OnDayEnd", EverydayEffect);
    }
    protected virtual void OnEndState() {
        Messenger.RemoveListener("OnDayEnd", EverydayEffect);
    }
    protected virtual void EverydayEffect() { }
    protected virtual void OnInteractWith(Character character) { }
    #endregion

    public bool HasActionInState(CharacterAction action, bool changeActionIfTrue = false) {
        foreach (CharacterAction currentAction in _actionRewardDictionary.Keys) {
            if(currentAction.actionType == action.actionType && currentAction.state.obj == action.state.obj) {
                if (changeActionIfTrue) {
                    //TODO: Change action of character to currentAction if it matches
                }
                return true;
            }
        }
        return false;
    }

    #region Actions
    public void AddNewAction(CharacterAction action, Reward reward) {
        _actionRewardDictionary.Add(action, reward);
    }
    public void RemoveAction(CharacterAction action) {
        _actionRewardDictionary.Remove(action);
    }
    public CharacterAction GetAction(ACTION_TYPE type) {
        for (int i = 0; i < _actionRewardDictionary.Keys.Count; i++) {
            CharacterAction currAction = _actionRewardDictionary.Keys.ElementAt(i);
            if (currAction.actionType == type) {
                return currAction;
            }
        }
        return null;
    }
    #endregion
}
