using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkSaveData {
    public int landmarkID;
    public string landmarkName;
    public Point locationCoordinates;
    public LANDMARK_TYPE landmarkType;
    public int civilianCount;
    public int chosenMonsterSet;

    public LandmarkSaveData(BaseLandmark landmark) {
        landmarkID = landmark.id;
        landmarkName = landmark.landmarkName;
        locationCoordinates = new Point(landmark.tileLocation.xCoordinate, landmark.tileLocation.yCoordinate);
        landmarkType = landmark.specificLandmarkType;
        civilianCount = landmark.civilianCount;
        if (landmark is MonsterSpawnerLandmark) {
            MonsterSet monsterChoices = (landmark as MonsterSpawnerLandmark).monsterChoices;
            if (monsterChoices == null) {
                chosenMonsterSet = -1;
            } else {
                LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkType);
                for (int i = 0; i < data.monsterSets.Count; i++) {
                    if (data.monsterSets[i] == monsterChoices) {
                        chosenMonsterSet = i;
                        break;
                    }
                }
            }
        } else {
            chosenMonsterSet = -1;
        }
    }
}
