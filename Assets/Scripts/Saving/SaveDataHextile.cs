using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataHextile {
    public int id;
    public int xCoordinate;
    public int yCoordinate;
    public string tileName;

    public float elevationNoise;
    public float moistureNoise;
    public float temperature;
    public BIOMES biomeType;
    public ELEVATION elevationType;

    public int landmarkID;
    //public int areaID;

    public void Save(HexTile tile) {
        id = tile.id;
        xCoordinate = tile.xCoordinate;
        yCoordinate = tile.yCoordinate;
        tileName = tile.tileName;
        elevationNoise = tile.elevationNoise;
        moistureNoise = tile.moistureNoise;
        temperature = tile.temperature;
        biomeType = tile.biomeType;
        elevationType = tile.elevationType;
        if(tile.landmarkOnTile != null) {
            landmarkID = tile.landmarkOnTile.id;
        } else {
            landmarkID = -1;
        }
        //if (tile.settlementOfTile != null) {
        //    areaID = tile.settlementOfTile.id;
        //} else {
        //    areaID = -1;
        //}
    }
    public void Load(HexTile tile) {
        tile.data.id = id;
        tile.data.xCoordinate = xCoordinate;
        tile.data.yCoordinate = yCoordinate;
        tile.data.tileName = tileName;
        tile.data.elevationNoise = elevationNoise;
        tile.data.moistureNoise = moistureNoise;
        tile.data.temperature = temperature;
        tile.data.biomeType = biomeType;
        tile.data.elevationType = elevationType;
    }
}
