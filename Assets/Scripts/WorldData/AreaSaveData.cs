using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSaveData {
    public int areaID;
    public string areaName;
    public AREA_TYPE areaType;
    public int coreTileID;
    public List<int> tileData; //list of tile id's that belong to this region
    public Color32 areaColor;

    public AreaSaveData(Area area) {
        areaID = area.id;
        areaName = area.name;
        areaType = area.areaType;
        coreTileID = area.coreTile.id;
        tileData = new List<int>();
        for (int i = 0; i < area.tiles.Count; i++) {
            HexTile currTile = area.tiles[i];
            tileData.Add(currTile.id);
        }
        areaColor = area.areaColor;
    }
}
