using UnityEngine;
using System.Collections;

public class DungeonLandmark : BaseLandmark {

    public DungeonLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = false;
    }
}
