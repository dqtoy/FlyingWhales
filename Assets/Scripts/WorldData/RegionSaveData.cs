using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RegionSaveData {
    public int regionID;
    public string regionName;
    public int centerTileID;
    public List<int> tileData; //list of tile id's that belong to this region
    public int owner;

    public RegionSaveData(Region region) {
        regionID = region.id;
        regionName = region.name;
        centerTileID = region.centerOfMass.id;
        ConstructTileData(region);
        ConstructOwnerData(region);
    }

    private void ConstructTileData(Region region) {
        tileData = new List<int>();
        for (int i = 0; i < region.tilesInRegion.Count; i++) {
            HexTile currTile = region.tilesInRegion[i];
            tileData.Add(currTile.id);
        }
    }

    private void ConstructOwnerData(Region region) {
        owner = (region.owner == null) ? -1 : region.owner.id;
    }
}
