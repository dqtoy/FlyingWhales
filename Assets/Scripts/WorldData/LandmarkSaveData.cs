using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkSaveData {
    public int landmarkID;
    public string landmarkName;
    public Point locationCoordinates;
    public LANDMARK_TYPE landmarkType;
    public int civilianCount;

    public LandmarkSaveData(BaseLandmark landmark) {
        landmarkID = landmark.id;
        landmarkName = landmark.landmarkName;
        locationCoordinates = new Point(landmark.tileLocation.xCoordinate, landmark.tileLocation.yCoordinate);
        landmarkType = landmark.specificLandmarkType;
        if (landmark is Settlement) {
            civilianCount = (landmark as Settlement).civilianCount;
        }
        
    }
}
