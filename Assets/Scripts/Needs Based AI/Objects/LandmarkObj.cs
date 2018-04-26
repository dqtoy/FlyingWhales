using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkObj : IObject {
    private string _objectName;
    private SPECIFIC_OBJECT_TYPE _specificObjType;
    private OBJECT_TYPE _objectType;
    private bool _isInvisible;
    private List<ObjectState> _states;

    private ObjectState _currentState;

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
    #endregion

    public LandmarkObj() {
        _objectName = "Landmark Object";
        _isInvisible = true;
    }

    #region Interface Requirements
    public void SetStates(List<ObjectState> states) {
        _states = states;
    }
    public void SetObjectName(string name) {
        _objectName = name;
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
    public IObject Clone() {
        LandmarkObj clone = new LandmarkObj();
        clone._specificObjType = this._specificObjType;
        clone._objectType = this._objectType;
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
