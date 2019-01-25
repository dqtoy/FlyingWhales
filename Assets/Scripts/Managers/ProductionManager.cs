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

	private Dictionary<WEAPON_TYPE, WeaponProduction> _weaponProductionsLookup;
	private Dictionary<ARMOR_TYPE, ArmorProduction> _armorProductionsLookup;
	private Dictionary<string, Construction> _constructionsLookup;
	private Dictionary<CHARACTER_ROLE, TrainingRole> _trainingRolesLookup;
	private Dictionary<CHARACTER_CLASS, TrainingClass> _trainingClassesLookup;

	public List<MATERIAL> weaponMaterials;
	public List<MATERIAL> armorMaterials;
	public List<MATERIAL> constructionMaterials;
	public List<MATERIAL> trainingMaterials;

	#region getters/setters
	public Dictionary<WEAPON_TYPE, WeaponProduction> weaponProductionsLookup{
		get { return _weaponProductionsLookup; }
	}
	public Dictionary<ARMOR_TYPE, ArmorProduction> armorProductionsLookup{
		get { return _armorProductionsLookup; }
	}
	public Dictionary<string, Construction> constructionsLookup{
		get { return _constructionsLookup; }
	}
	public Dictionary<CHARACTER_ROLE, TrainingRole> trainingRolesLookup{
		get { return _trainingRolesLookup; }
	}
	public Dictionary<CHARACTER_CLASS, TrainingClass> trainingClassesLookup{
		get { return _trainingClassesLookup; }
	}

	#endregion
	void Awake(){
		Instance = this;
	}
	internal void Initialize(){
		ConstructProductions ();
	}
	private void ConstructProductions(){
		_weaponProductionsLookup = new Dictionary<WEAPON_TYPE, WeaponProduction> ();
		_armorProductionsLookup = new Dictionary<ARMOR_TYPE, ArmorProduction> ();
		_constructionsLookup = new Dictionary<string, Construction> ();
		_trainingRolesLookup = new Dictionary<CHARACTER_ROLE, TrainingRole> ();
		_trainingClassesLookup = new Dictionary<CHARACTER_CLASS, TrainingClass> ();

		WeaponProduction[] arrWeapProduction = weaponProductionGO.GetComponents<WeaponProduction> ();
		for (int i = 0; i < arrWeapProduction.Length; i++) {
			_weaponProductionsLookup.Add (arrWeapProduction [i].weaponType, arrWeapProduction [i]);
		}

		ArmorProduction[] arrArmorProduction = armorProductionGO.GetComponents<ArmorProduction> ();
		for (int i = 0; i < arrArmorProduction.Length; i++) {
			_armorProductionsLookup.Add (arrArmorProduction [i].armorType, arrArmorProduction [i]);
		}

		Construction[] arrConstruction = constructionGO.GetComponents<Construction> ();
		//for (int i = 0; i < arrConstruction.Length; i++) {
		//	_constructionsLookup.Add (arrConstruction [i].structure.name, arrConstruction [i]);
		//}

		TrainingRole[] arrTrainingRole = trainingRoleGO.GetComponents<TrainingRole> ();
		for (int i = 0; i < arrTrainingRole.Length; i++) {
			_trainingRolesLookup.Add (arrTrainingRole [i].roleType, arrTrainingRole [i]);
		}

		TrainingClass[] arrTrainingClass = trainingClassGO.GetComponents<TrainingClass> ();
		for (int i = 0; i < arrTrainingClass.Length; i++) {
			_trainingClassesLookup.Add (arrTrainingClass [i].classType, arrTrainingClass [i]);
		}
	}
	internal WeaponProduction GetWeaponProduction(WEAPON_TYPE weaponType){
		if(_weaponProductionsLookup.ContainsKey(weaponType)){
			return _weaponProductionsLookup [weaponType];
		}
		return null;
	}
	internal ArmorProduction GetArmorProduction(ARMOR_TYPE armorType){
		if(_armorProductionsLookup.ContainsKey(armorType)){
			return _armorProductionsLookup [armorType];
		}
		return null;
	}
	internal Construction GetConstruction(string structure){
		if(_constructionsLookup.ContainsKey(structure)){
			return _constructionsLookup [structure];
		}
		return null;
	}
    internal Construction GetConstructionDataForCity() {
        return GetConstruction("BASIC CITY");
    }
	internal TrainingRole GetTrainingRole(CHARACTER_ROLE role){
		if(_trainingRolesLookup.ContainsKey(role)){
			return _trainingRolesLookup [role];
		}
		return null;
	}
	internal TrainingClass GetTrainingClass(CHARACTER_CLASS classType){
		if(_trainingClassesLookup.ContainsKey(classType)){
			return _trainingClassesLookup [classType];
		}
		return null;
	}

    internal bool CanMaterialBeUsedForConstruction(MATERIAL material) {
		if (constructionMaterials.Contains(material)) {
			return true;
		}
        return false;
    }
    internal bool CanMaterialBeUsedForTraining(MATERIAL material) {
		if (trainingMaterials.Contains(material)) {
			return true;
		}
        return false;
    }
}
