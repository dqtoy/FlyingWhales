using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheNeedles : BaseLandmark {

    public TheNeedles(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheNeedles(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
