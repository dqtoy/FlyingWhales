using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheProfane : BaseLandmark {

    public TheProfane(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheProfane(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
