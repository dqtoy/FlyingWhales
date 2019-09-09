using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCrypt : BaseLandmark {
    public TheCrypt(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheCrypt(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
