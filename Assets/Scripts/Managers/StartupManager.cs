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
        //EncounterPartyManager.Instance.Initialize ();
		MaterialManager.Instance.Initialize();
		ProductionManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
		//TaskManager.Instance.Initialize ();

        LevelLoaderManager.UpdateLoadingInfo("Initializing World...");
        if(SaveManager.Instance.currentSave != null) {

        }
        if (SaveManager.Instance.currentSave != null) {
            Debug.Log("Loading world from current saved data...");
            this.mapGenerator.InitializeWorld(SaveManager.Instance.currentSave);
        } else if (WorldConfigManager.Instance != null && WorldConfigManager.Instance.loadedData != null) {
            Debug.Log("Loading world from data...");
            this.mapGenerator.InitializeWorld(WorldConfigManager.Instance.loadedData);
        } else {
            Debug.Log("Generating random world...");
            this.mapGenerator.InitializeWorld();
        }

        //if (SaveManager.Instance == null) {
        //    this.mapGenerator.InitializeWorld();
        //} else {
        //    if (SaveManager.Instance.currentSave == null) {
        //        this.mapGenerator.InitializeWorld();
        //    } else {
        //        this.mapGenerator.LoadWorld(SaveManager.Instance.currentSave);
        //    }
        //}
    }
}
