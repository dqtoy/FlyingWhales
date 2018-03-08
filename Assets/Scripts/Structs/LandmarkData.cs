using UnityEngine;
using System.Collections;

[System.Serializable]
public class LandmarkData {
    public LANDMARK_TYPE landmarkType;
    public int durabilityModifier;
    public int appearanceWeight;
    public bool isUnique;
    public MATERIAL[] possibleMaterials; //Possible materials that this landmark can be made of (this affects the landmarks durability)
}
