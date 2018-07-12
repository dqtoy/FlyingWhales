using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemObj : IObject {
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private List<ObjectState> _states;

    private string _objectName;
    [System.NonSerialized] private ObjectState _currentState;
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
    public ILocation specificLocation {
        get { return objectLocation; }
    }
    public Dictionary<RESOURCE, int> resourceInventory {
        get { return null; }
    }
    public RESOURCE madeOf {
        get { return RESOURCE.NONE; }
    }
    #endregion

    public ItemObj() {

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
    public IObject Clone() {
        ItemObj clone = new ItemObj();
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
