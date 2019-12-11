using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct AreaData {
    [FormerlySerializedAs("areaType")] public LOCATION_TYPE locationType;
    public BASE_AREA_TYPE baseAreaType;
}
