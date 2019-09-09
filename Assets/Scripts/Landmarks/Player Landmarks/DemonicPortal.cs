using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicPortal : BaseLandmark {

    public DemonicPortal(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public DemonicPortal(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
