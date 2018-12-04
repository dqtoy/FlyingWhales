using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MonsterObj : ICharacterObject {
    private OBJECT_TYPE _objectType;
    private bool _isInvisible;
    private List<ObjectState> _states;

    private MonsterParty _party;
    private string _objectName;
    [NonSerialized] private ObjectState _currentState;
    private BaseLandmark _objectLocation;

    #region getters/setters
    public Party iparty {
        get { return _party; }
    }
    public string objectName {
        get { return _objectName; }
    }
    public OBJECT_TYPE objectType {
        get { return _objectType; }
    }
    public List<ObjectState> states {
        get { return _states; }
    }
    public ObjectState currentState {
        get { return _currentState; }
    }
    public bool isInvisible {
        get { return _isInvisible; }
    }
    public BaseLandmark objectLocation {
        get { return _party.landmarkLocation; }
    }
    public ILocation specificLocation {
        get { return _party.specificLocation; }
    }
    public RESOURCE madeOf {
        get { return RESOURCE.NONE; }
    }
    public Dictionary<RESOURCE, int> resourceInventory {
        get { return null; }
    }
    #endregion


    public MonsterObj(MonsterParty party) {
        _objectType = OBJECT_TYPE.MONSTER;
        _party = party;
    }

    #region Interface Requirements
    public void SetStates(List<ObjectState> states, bool autoChangeState = true) {
        _states = states;
        if (autoChangeState) {
            ChangeState(states[0]);
        }
    }
    public void SetObjectName(string name) {
        _objectName = name;
    }
    public void SetObjectLocation(BaseLandmark newLocation) {
        _objectLocation = newLocation;
    }
    public void SetIsInvisible(bool state) {
        _isInvisible = state;
    }
    public void SetMonster(MonsterParty party) {
        _party = party;
    }
    public void ChangeState(ObjectState state) {
        if (_currentState != null) {
            _currentState.OnEndState();
        }
        _currentState = state;
        _currentState.OnStartState();
    }
    public void StartState(ObjectState state) {

    }
    public void EndState(ObjectState state) {

    }
    public ObjectState GetState(string name) {
        for (int i = 0; i < _states.Count; i++) {
            if (_states[i].stateName == name) {
                return _states[i];
            }
        }
        return null;
    }
    public bool OwnsAction(CharacterAction action) {
        for (int i = 0; i < states.Count; i++) {
            ObjectState currState = states[i];
            for (int j = 0; j < currState.actions.Count; j++) {
                CharacterAction currAction = currState.actions[j];
                if (currAction == action) {
                    return true;
                }
            }
        }
        return false;
    }
    public IObject Clone() {
        MonsterObj clone = new MonsterObj(_party);
        clone.SetObjectName(this._objectName);
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        List<ObjectState> states = new List<ObjectState>();
        for (int i = 0; i < this.states.Count; i++) {
            ObjectState currState = this.states[i];
            ObjectState clonedState = currState.Clone(clone);
            states.Add(clonedState);
            //if (this.currentState == currState) {
            //    clone.ChangeState(clonedState);
            //}
        }
        clone.SetStates(states);
        return clone;
    }
    public void OnAddToLandmark(BaseLandmark newLocation) {
        SetObjectLocation(newLocation);
    }
    public void AdjustResource(RESOURCE resource, int amount) {
        //Don't have resource inventory
    }
    #endregion
}
