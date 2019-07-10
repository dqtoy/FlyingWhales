using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;

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
        if (WorldConfigManager.Instance == null || WorldConfigManager.Instance.loadedData == null) {
            Debug.Log("Generating random world...");
            this.mapGenerator.InitializeWorld();
        } else {
            Debug.Log("Loading world from data...");
            this.mapGenerator.InitializeWorld(WorldConfigManager.Instance.loadedData);
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
