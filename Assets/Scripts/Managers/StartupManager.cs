using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;

	void Start(){
        //LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        DataConstructor.Instance.InitializeData();
        ECS.CombatManager.Instance.Initialize();
        EncounterPartyManager.Instance.Initialize ();
		MaterialManager.Instance.Initialize ();
		ProductionManager.Instance.Initialize ();
		TaskManager.Instance.Initialize ();

        //LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing World...");
        if(SaveManager.Instance == null) {
            this.mapGenerator.InitializeWorld();
        } else {
            if (SaveManager.Instance.currentSave == null) {
                this.mapGenerator.InitializeWorld();
            } else {
                this.mapGenerator.LoadWorld(SaveManager.Instance.currentSave);
            }
        }
    }
}
