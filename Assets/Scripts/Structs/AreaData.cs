using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AreaData {
    public AREA_TYPE areaType;
    public List<LANDMARK_TYPE> allowedLandmarkTypes;
}
