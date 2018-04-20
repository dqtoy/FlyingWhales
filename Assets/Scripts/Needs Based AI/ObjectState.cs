using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ObjectState {
    protected IObject _object;
    protected Dictionary<CharacterAction, Reward> _actionRewardDictionary;
    protected float _foodAdvertisementMod;
    protected float _energyAdvertisementMod;
    protected float _joyAdvertisementMod;
    protected float _prestigeAdvertisementMod;

    #region getters/setters
    public IObject obj {
        get { return _object; }
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

    public ObjectState(IObject obj) {
        _object = obj;
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
}
