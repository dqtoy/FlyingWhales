using UnityEngine;
using System.Collections;

public class LairLandmark : BaseLandmark {

    public LairLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = false;
    }

    public LairLandmark(HexTile location, LandmarkSaveData data) : base(location, data) {
        _canBeOccupied = false;
    }
}
