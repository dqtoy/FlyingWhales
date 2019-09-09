using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheKennel : BaseLandmark {

    public TheKennel(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheKennel(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
