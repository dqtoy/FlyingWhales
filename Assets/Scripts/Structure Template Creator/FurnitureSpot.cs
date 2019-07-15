using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FurnitureSpot {

    public Vector3Int location; //where in the template grid is this furniture spot placed
    public FURNITURE_TYPE[] allowedFurnitureTypes;
    public List<FurnitureSetting> furnitureSettings;

    public override string ToString() {
        string summary = string.Empty;
        for (int i = 0; i < allowedFurnitureTypes.Length; i++) {
            summary += "|" + allowedFurnitureTypes[i].ToString() + "|";
        }
        return summary;
    }

    public FurnitureSetting GetFurnitureSettings(FURNITURE_TYPE type) {
        for (int i = 0; i < furnitureSettings.Count; i++) {
            FurnitureSetting currSetting = furnitureSettings[i];
            if (currSetting.type == type) {
                return currSetting;
            }
        }
        throw new System.Exception("No Setting for furniture type " + type.ToString());
    }
}
