using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProductionManager : MonoBehaviour {
	public static ProductionManager Instance;
	public GameObject weaponProductionGO;
	public GameObject armorProductionGO;
	public GameObject constructionGO;
	public GameObject trainingRoleGO;
	public GameObject trainingClassGO;

	private WeaponProduction[] _weaponProductions;
	private ArmorProduction[] _armorProductions;
	private Construction[] _constructions;
	private TrainingRole[] _trainingRoles;
	private TrainingClass[] _trainingClasses;

	void Awake(){
		Instance = this;
	}

	private void ConstructProductions(){
		_weaponProductions = weaponProductionGO.GetComponents<WeaponProduction> ();
		_armorProductions = armorProductionGO.GetComponents<ArmorProduction> ();
		_constructions = constructionGO.GetComponents<Construction> ();
		_trainingRoles = trainingRoleGO.GetComponents<TrainingRole> ();
		_trainingClasses = trainingClassGO.GetComponents<TrainingClass> ();

	}
}
