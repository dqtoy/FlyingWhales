using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;

    void Awake() {
        Messenger.Cleanup();
    }
    void Start(){
        LevelLoaderManager.SetLoadingState(true);
        LevelLoaderManager.UpdateLoadingInfo("Initializing Data...");
        DataConstructor.Instance.InitializeData();
        CombatManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
        TimerHubUI.Instance.Initialize();

        LevelLoaderManager.UpdateLoadingInfo("Initializing World...");
        if (SaveManager.Instance.currentSave != null) {
            Debug.Log("Loading world from current saved data...");
            this.mapGenerator.InitializeWorld(SaveManager.Instance.currentSave);
        } else {
            Debug.Log("Generating random world...");
            this.mapGenerator.InitializeWorld();
        }
    }
}
