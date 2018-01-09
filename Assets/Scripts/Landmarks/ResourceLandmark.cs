using UnityEngine;
using System.Collections;

public class ResourceLandmark : BaseLandmark {

    public ResourceLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
    }
}
