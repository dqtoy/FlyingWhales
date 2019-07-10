using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;

    [System.NonSerialized]public WorldSaveData loadedData = null; //Used for saved worlds

    [Header("Grid")]
    public int gridSizeX;
    public int gridSizeY;
    public int regionCount;
    [Header("Settlements")]
    [Tooltip("Minimum number of settlements to generate")]
    public int minSettltementCount;
    [Tooltip("Maximum number of settlements to generate")]
    public int maxSettltementCount;
    [Tooltip("Minimum number of citizens to generate per settlement")]
    public int minCitizenCount;
    [Tooltip("Maximum number of citizens to generate per settlement")]
    public int maxCitizenCount;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetDataToUse(WorldSaveData data) {
        loadedData = data;
    }
}
