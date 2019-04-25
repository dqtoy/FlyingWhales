using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureConnector {

    public Vector3Int location; //where in the template grid is this connector placed
    public Cardinal_Direction neededDirection; //The connection type that this connector will accept
    public STRUCTURE_TYPE allowedStructureType; //The type of structures that can connect to this

    public bool isOpen; //is this connection still open?

    public Vector3Int Difference(StructureConnector otherConnector) {
        return new Vector3Int(location.x - otherConnector.location.x, location.y - otherConnector.location.y, 0);
    }

    public void SetIsOpen(bool state) {
        isOpen = state;
    }
}
