using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ObjectSpawnLandmark {
    public ObjectComponent obj;
    public ObjectComponent fallbackObject;
    public int spawnChance;
}
