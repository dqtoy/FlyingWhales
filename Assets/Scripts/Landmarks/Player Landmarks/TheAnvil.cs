using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheAnvil : BaseLandmark {

    public TheAnvil(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheAnvil(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
