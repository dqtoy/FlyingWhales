using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;
    public Initializer initializer;

    void Awake() {
        Messenger.Cleanup();
    }
    void Start(){
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        LevelLoaderManager.SetLoadingState(true);
        LevelLoaderManager.UpdateLoadingInfo("Initializing Data...");
        initializer.InitializeDataBeforeWorldCreation();

        LevelLoaderManager.UpdateLoadingInfo("Initializing World...");
        if (SaveManager.Instance.currentSave != null) {
            Debug.Log("Loading world from current saved data...");
            this.mapGenerator.InitializeWorld(SaveManager.Instance.currentSave);
        } else {
            Debug.Log("Generating random world...");
            this.mapGenerator.InitializeWorld();
        }
    }

    private void OnGameLoaded() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        // initializer.InitializeDataAfterWorldCreation();
    }
}
