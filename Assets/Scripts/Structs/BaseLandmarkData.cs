using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct BaseLandmarkData {
    public BASE_LANDMARK_TYPE baseLandmarkType;
    public List<LANDMARK_TAG> baseLandmarkTags;
}
