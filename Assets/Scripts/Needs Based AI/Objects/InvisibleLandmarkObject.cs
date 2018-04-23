using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleLandmarkObject : IObject {

    private List<ObjectState> _states;
    private ObjectState _currentState;
    private bool _isInvisible;
    private int _foodAdvertisementValue;
    private int _energyAdvertisementValue;
    private int _joyAdvertisementValue;
    private int _prestigeAdvertisementValue;

    #region getters/setters
    public string objectName {
        get { return "Invisible Landmark Object"; }
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
    public int foodAdvertisementValue {
        get { return _foodAdvertisementValue; }
    }
    public int energyAdvertisementValue {
        get { return _energyAdvertisementValue; }
    }
    public int joyAdvertisementValue {
        get { return _joyAdvertisementValue; }
    }
    public int prestigeAdvertisementValue {
        get { return _prestigeAdvertisementValue; }
    }
    #endregion

    public InvisibleLandmarkObject() {
        _states = new List<ObjectState>();
        ConstructStates();
    }

    private void ConstructStates() {
        ObjectState normalState = new ObjectState(this, "Normal");
        normalState.AddNewAction(new MoveTo(normalState), new Reward());

        _states.Add(normalState);
    }


    public void ChangeState(ObjectState state) {
        _currentState = state;
    }
}
