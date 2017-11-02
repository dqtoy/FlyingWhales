using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct HexRoads {
    public HEXTILE_DIRECTION from;
    public List<RoadObject> destinations;
}
