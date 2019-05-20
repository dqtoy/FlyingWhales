using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FurnitureSpot {

    public Vector3Int location; //where in the template grid is this furniture spot placed
    public List<FURNITURE_TYPE> allowedFurnitureTypes;

    public override string ToString() {
        string summary = string.Empty;
        for (int i = 0; i < allowedFurnitureTypes.Count; i++) {
            summary += "|" + allowedFurnitureTypes[i].ToString() + "|";
        }
        return summary;
    }
}
