using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkObj : IObject {
    private string _objectName;
    private OBJECT_TYPE _objectType;
    private bool _isInvisible;
    private List<ObjectState> _states;

    private ObjectState _currentState;

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
    #endregion

    public LandmarkObj() {
        _objectName = "Landmark Object";
        _isInvisible = true;
    }

    public void ChangeState(ObjectState state) {

    }
}
