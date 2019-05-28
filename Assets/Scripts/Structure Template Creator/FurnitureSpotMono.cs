﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class FurnitureSpotMono : MonoBehaviour {

    [SerializeField] private TextMeshPro furnitureSummary;

    public FURNITURE_TYPE[] allowedFurnitureTypes;
    public List<FurnitureSetting> furnitureSettings;

    private void Update() {
        string summary = string.Empty;
        for (int i = 0; i < allowedFurnitureTypes.Length ; i++) {
            summary += allowedFurnitureTypes[i].ToString() + ", ";
        }
        this.name = summary;
        furnitureSummary.text = summary;
    }
}

[System.Serializable]
public class FurnitureSetting {
    public FURNITURE_TYPE type;
    public Vector3 rotation;
    public string tileAssetName;
}