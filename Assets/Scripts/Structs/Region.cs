using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Region {
    public HexTile centerOfMass;
    public List<HexTile> tilesInRegion;
    public Color regionColor;

    public Region(HexTile centerOfMass) {
        this.centerOfMass = centerOfMass;
        tilesInRegion = new List<HexTile>();
        regionColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
    }

    public void AddTile(HexTile tile) {
        tilesInRegion.Add(tile);
    }

    public void ReComputeCenterOfMass() {
        int maxXCoordinate = tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;
        
        centerOfMass = GridMap.Instance.map[midPointX, midPointY];
    }

    public void ResetTilesInRegion() {
        tilesInRegion.Clear();
    }

    public void RevalidateCenterOfMass() {
        if(centerOfMass.elevationType != ELEVATION.PLAIN || centerOfMass.specialResource != RESOURCE.NONE) {
            centerOfMass = tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x.specialResource == RESOURCE.NONE)
                .OrderBy(x => x.GetDistanceTo(centerOfMass)).FirstOrDefault();
            if(centerOfMass == null) {
                throw new System.Exception("center of mass is null!");
            }
        }
    }
}
