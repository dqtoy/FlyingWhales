using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCObj : IObject {
    [SerializeField] private SPECIFIC_OBJECT_TYPE _specificObjType;
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private List<ObjectState> _states;

    private string _objectName;
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

    public NPCObj() {

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
        NPCObj clone = new NPCObj();
        clone.SetObjectName (this._objectName);
        clone._specificObjType = this.specificObjType;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._states = new List<ObjectState>();
        for (int i = 0; i < this.states.Count; i++) {
            ObjectState currState = this.states[i];
            clone._states.Add(currState.Clone(clone));
        }
        return clone;
    }
    #endregion
}
