using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LandmarkSaveData {
    public int landmarkID;
    public string landmarkName;
    public Point locationCoordinates;
    public LANDMARK_TYPE landmarkType;
    public int civilianCount;
    public int chosenMonsterSet;
    public List<string> items;
    public int[] defenders;

    public LandmarkSaveData(BaseLandmark landmark) {
        landmarkID = landmark.id;
        landmarkName = landmark.landmarkName;
        locationCoordinates = new Point(landmark.tileLocation.xCoordinate, landmark.tileLocation.yCoordinate);
        landmarkType = landmark.specificLandmarkType;
        civilianCount = landmark.civilianCount;
        items = new List<string>(landmark.itemsInLandmark.Select(x => x.itemName));
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

        defenders = new int[LandmarkManager.MAX_DEFENDERS];
        for (int i = 0; i < defenders.Length; i++) {
            if (landmark.defenders == null || landmark.defenders.icharacters.ElementAtOrDefault(i) == null) {
                defenders[i] = -1;
            } else {
                defenders[i] = landmark.defenders.icharacters[i].id;
            }
        }
    }
}
