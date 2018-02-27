using UnityEngine;
using System.Collections;

[System.Serializable]
public struct LandmarkData {
    public LANDMARK_TYPE landmarkType;
    public BASE_LANDMARK_TYPE baseType;
    public int appearanceWeight;
    public bool isUnique;
    public MATERIAL[] possibleMaterials; //Possible materials that this landmark can be made of (this affects the landmarks durability)
}
