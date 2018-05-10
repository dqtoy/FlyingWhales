﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

[System.Serializable]
public class CharacterObj : IObject {
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private List<ObjectState> _states;
    private Dictionary<RESOURCE, int> _resourceInventory;

    private Character _character;
    private string _objectName;
    [NonSerialized] private ObjectState _currentState;
    private BaseLandmark _objectLocation;

    #region getters/setters
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
        get { return _objectLocation; }
    }
    public Dictionary<RESOURCE, int> resourceInventory {
        get { return _resourceInventory; }
    }
    public RESOURCE madeOf {
        get { return RESOURCE.NONE; }
    }
    #endregion


    public CharacterObj(Character character) {
        _character = character;
        ConstructResourceInventory();
    }

    #region Interface Requirements
    public void SetStates(List<ObjectState> states) {
        _states = states;
        ChangeState(states[0]);
    }
    public void SetObjectName(string name) {
        _objectName = name;
    }
    public void SetObjectLocation(BaseLandmark newLocation) {
        _objectLocation = newLocation;
    }
    public void ChangeState(ObjectState state) {
        if (_currentState != null) {
            _currentState.OnEndState();
        }
        _currentState = state;
        _currentState.OnStartState();
    }
    public ObjectState GetState(string name) {
        for (int i = 0; i < _states.Count; i++) {
            if (_states[i].stateName == name) {
                return _states[i];
            }
        }
        return null;
    }
    public IObject Clone() {
        CharacterObj clone = new CharacterObj(_character);
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
    #endregion

    #region Resource Inventory
    private void ConstructResourceInventory() {
        _resourceInventory = new Dictionary<RESOURCE, int>();
        RESOURCE[] allResources = Utilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < allResources.Length; i++) {
            if (allResources[i] != RESOURCE.NONE) {
                _resourceInventory.Add(allResources[i], 0);
            }
        }
    }
    public void AdjustResource(RESOURCE resource, int amount) {
        _resourceInventory[resource] += amount;
        if(_resourceInventory[resource] < 0) {
            _resourceInventory[resource] = 0;
        }
    }
    public void TransferResourceTo(RESOURCE resource, int amount, StructureObj target) {
        AdjustResource(resource, -amount);
        target.AdjustResource(resource, amount);
    }
    public void TransferResourceTo(RESOURCE resource, int amount, CharacterObj target) {
        AdjustResource(resource, -amount);
        target.AdjustResource(resource, amount);
    }
    public void TransferResourceTo(RESOURCE resource, int amount, LandmarkObj target) {
        AdjustResource(resource, -amount);
        target.AdjustResource(resource, amount);
    }
    public int GetTotalCivilians() {
        int total = 0;
        foreach (KeyValuePair<RESOURCE, int> kvp in _resourceInventory) {
            if (kvp.Key.ToString().Contains("CIVILIAN")) {
                total += kvp.Value;
            }
        }
        return total;
    }
    #endregion
}
