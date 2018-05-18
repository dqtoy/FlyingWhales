using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HextileSave {
    public int xCoordinate;
    public int yCoordinate;
    public BaseLandmark landmark;

    public void SaveTile(HexTile tile) {
        xCoordinate = tile.xCoordinate;
        yCoordinate = tile.yCoordinate;
        landmark = tile.landmarkOnTile;
    }
}
