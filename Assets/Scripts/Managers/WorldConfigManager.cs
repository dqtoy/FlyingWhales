using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;

    [System.NonSerialized]public WorldSaveData loadedData = null; //Used for saved worlds

    public int gridSizeX;
    public int gridSizeY;
    public int settltementCount; //how many settlements should be generated

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
