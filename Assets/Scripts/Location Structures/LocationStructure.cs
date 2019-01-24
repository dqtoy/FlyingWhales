using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocationStructure {

    [System.NonSerialized] private Area _location;
    public STRUCTURE_TYPE structureType { get; private set; }
    public bool isInside { get; private set; }

    #region getters
    public Area location {
        get { return _location; }
    }
    #endregion

    public LocationStructure(STRUCTURE_TYPE structureType, Area location, bool isInside = true) {
        this.structureType = structureType;
        this._location = location;
        this.isInside = isInside;
    }

    public void SetInsideState(bool isInside) {
        this.isInside = isInside;
    }

    public void DestroyStructure() {
        _location.RemoveStructure(this);
    }
}
