using UnityEngine;
using System.Collections;

public class LairLandmark : BaseLandmark {

    public LairLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType, MATERIAL materialMadeOf) : base(location, specificLandmarkType, materialMadeOf) {
        _canBeOccupied = false;
    }
}
