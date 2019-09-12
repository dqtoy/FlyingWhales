using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheFingers : BaseLandmark {
    public bool hasActivatedCreateChaoticEvents { get; private set; }
    public int currentCreateTick { get; private set; }
    public int createDuration { get; private set; }

    public TheFingers(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheFingers(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
