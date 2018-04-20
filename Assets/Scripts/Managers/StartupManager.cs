using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;

	void Start(){
        DataConstructor.Instance.InitializeData();
        ECS.CombatManager.Instance.Initialize();
        EncounterPartyManager.Instance.Initialize ();
		MaterialManager.Instance.Initialize ();
		ProductionManager.Instance.Initialize ();
		TaskManager.Instance.Initialize ();

		this.mapGenerator.InitializeWorld ();
	}
}
