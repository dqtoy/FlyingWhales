using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance = null;

    public WorldSaveData loadedData = null;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetDataToUse(WorldSaveData data) {
        loadedData = data;
    }
}
