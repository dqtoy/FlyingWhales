using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheFingers : BaseLandmark {

    public TheFingers(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheFingers(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
