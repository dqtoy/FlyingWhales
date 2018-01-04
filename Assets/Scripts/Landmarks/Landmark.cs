using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Landmark : Settlement {
    protected HexTile _location;
    protected LANDMARK_TYPE _landmarkType;
    protected List<object> _connections;
    protected LandmarkObject _landmarkObject;

    #region getters/setters
    internal HexTile location {
        get { return _location; }
    }
    internal LANDMARK_TYPE landmarkType {
        get { return _landmarkType; }
    }
    internal List<object> connections {
        get { return _connections; }
    }
    internal LandmarkObject landmarkObject {
        get { return _landmarkObject; }
    }
    #endregion

    public Landmark(HexTile location, LANDMARK_TYPE landmarkType) {
        _location = location;
        _landmarkType = landmarkType;
        _connections = new List<object>();
    }

    public void AddConnection(object connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }

    internal void SetLandmarkObject(LandmarkObject landmarkObject) {
        _landmarkObject = landmarkObject;
        _landmarkObject.SetLandmark(this);
    }
}
