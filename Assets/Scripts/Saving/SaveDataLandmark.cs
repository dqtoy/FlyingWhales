using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataLandmark {
    public int id;
    public string landmarkName;
    public LANDMARK_TYPE landmarkType;

    public void Save(BaseLandmark landmark) {
        id = landmark.id;
        landmarkName = landmark.landmarkName;
        landmarkType = landmark.specificLandmarkType;
    }
    public void Load(HexTile tile) {
        tile.CreateLandmarkOfType(this);
    }
}
