using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;

	void Start(){
		LocalizationManager.Instance.Initialize ();
		ItemManager.Instance.Initialize ();
		SkillManager.Instance.Initialize ();
		ECS.CombatPrototypeManager.Instance.Initialize ();
		EncounterPartyManager.Instance.Initialize ();
		CharacterManager.Instance.ConstructTraitDictionary();
		MaterialManager.Instance.Initialize ();
		ProductionManager.Instance.Initialize ();
		TaskManager.Instance.Initialize ();

		this.mapGenerator.InitializeWorld ();
	}
}
