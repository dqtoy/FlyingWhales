using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePit : BaseLandmark {

    public ThePit(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public ThePit(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
