using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnerLandmark : BaseLandmark {

    private int _monsterSpawnCooldown;
    private MonsterSet _monsterChoices;

    #region getters/setters
    public MonsterSet monsterChoices {
        get { return _monsterChoices; }
    }
    public int monsterSpawnCooldown {
        get { return _monsterSpawnCooldown; }
    }
    #endregion
    public MonsterSpawnerLandmark(HexTile location, LANDMARK_TYPE landmarkType) : base (location, landmarkType) { }
    public MonsterSpawnerLandmark(HexTile location, LandmarkSaveData data) : base(location, data) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(data.landmarkType);
    }
    public void SetMonsterChoices(MonsterSet chosenSet) {
        _monsterChoices = chosenSet;
    }
}
