using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StructureObj : IObject {
    public Action onHPReachedZero;
    public Action onHPReachedFull;

    [SerializeField] private SPECIFIC_OBJECT_TYPE _specificObjType;
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private int _maxHP;
    private List<ObjectState> _states;

    private string _objectName;
    private ObjectState _currentState;
    private BaseLandmark _objectLocation;
    private int _currentHP;

    #region getters/setters
    public string objectName {
        get { return _objectName; }
    }
    public SPECIFIC_OBJECT_TYPE specificObjType {
        get { return _specificObjType; }
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
    public int maxHP {
        get { return _maxHP; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public BaseLandmark objectLocation {
        get { return _objectLocation; }
    }
    #endregion

    public StructureObj() {
    }

    #region Interface Requirements
    public void SetStates(List<ObjectState> states) {
        _states = states;
    }
    public void SetObjectName(string name) {
        _objectName = name;
    }
    public void SetObjectLocation(BaseLandmark newLocation) {
        _objectLocation = newLocation;
    }
    public void ChangeState(ObjectState state) {
        _currentState.OnEndState();
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
    public void AdjustHP(int amount) {
        //When hp reaches 0 or 100 a function will be called
        int previousHP = _currentHP;
        _currentHP += amount;
        _currentHP = Mathf.Clamp(_currentHP, 0, 100);
        if (previousHP != _currentHP) {
            if (_currentHP == 0 && onHPReachedZero != null) {
                onHPReachedZero();
            } else if (_currentHP == 100 && onHPReachedFull != null) {
                onHPReachedFull();
            }
        }
    }
    public IObject Clone() {
        StructureObj clone = new StructureObj();
        clone.SetObjectName(this._objectName);
        clone._specificObjType = this._specificObjType;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._maxHP = this.maxHP;
        clone._states = new List<ObjectState>();
        for (int i = 0; i < this.states.Count; i++) {
            ObjectState currState = this.states[i];
            ObjectState clonedState = currState.Clone(clone);
            clone._states.Add(clonedState);
            if (this.currentState == currState) {
                clone.ChangeState(clonedState);
            }
        }
        return clone;
    }
    #endregion
}
