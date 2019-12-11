using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TownMapSettings {

    public Point size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] groundWallTiles;
    public TileTemplateData[] structureTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public List<BuildingSpotData> buildSpots;

    public TownMapSettings() {
        buildSpots = new List<BuildingSpotData>();
    }

    public void LogInfo() {
        string info = "Town Map Info: " + size.ToString();
        info += "\nGround tiles: " + groundTiles.Length.ToString();
        info += "\nStructure tiles: " + structureTiles.Length.ToString();
        info += "\nObejct tiles: " + objectTiles.Length.ToString();
        info += "\nDetail tiles: " + detailTiles.Length.ToString();
        //Debug.Log(info);
    }

}
