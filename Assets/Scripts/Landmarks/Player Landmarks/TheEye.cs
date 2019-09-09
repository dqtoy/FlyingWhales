using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheEye : BaseLandmark {

    public TheEye(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheEye(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
