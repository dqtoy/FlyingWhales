using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct HexRoads {
    public HEXTILE_DIRECTION from;
    public GameObject directionGO; //A Gameobject that contains the sprite going to the specified direction
    public List<RoadObject> destinations;
}
