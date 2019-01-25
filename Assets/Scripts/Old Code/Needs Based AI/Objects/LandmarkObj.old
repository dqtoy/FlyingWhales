using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LandmarkObj : IObject {
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    private List<ObjectState> _states;

    [System.NonSerialized] private ObjectState _currentState;
    private string _objectName;
    private Dictionary<RESOURCE, int> _resourceInventory;
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
        get { return _resourceInventory; }
    }
    public RESOURCE madeOf {
        get { return RESOURCE.NONE; }
    }
    #endregion

    public LandmarkObj(BaseLandmark landmark) {
        //_objectName = "Landmark Object";
        //_isInvisible = true;
        //SetObjectLocation(landmark);
        //List<ObjectState> states = new List<ObjectState>();
        //ObjectState defaultState = new ObjectState(this);
        //defaultState.SetName("Default");
        //states.Add(defaultState);

        //IdleAction idle = new IdleAction(defaultState);
        //CharacterActionData data = idle.actionData;
        //data.duration = 5;
        //idle.SetActionData(data);

        //defaultState.AddNewAction(idle);
        //SetStates(states);
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
        LandmarkObj clone = new LandmarkObj(_objectLocation);
        clone._isInvisible = true;
        clone._objectType = this._objectType;
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
