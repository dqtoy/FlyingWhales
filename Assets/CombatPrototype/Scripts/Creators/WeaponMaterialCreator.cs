#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WeaponMaterialComponent))]
public class WeaponMaterialCreator : Editor {

	WeaponMaterialComponent weaponMaterialComponent;

	public override void OnInspectorGUI() {
		if(weaponMaterialComponent == null){
			weaponMaterialComponent = (WeaponMaterialComponent)target;
		}
		weaponMaterialComponent.material = (MATERIAL)EditorGUILayout.EnumPopup ("Material : ", weaponMaterialComponent.material);
		weaponMaterialComponent.power = EditorGUILayout.IntField ("Normal Power : ", weaponMaterialComponent.power);
		weaponMaterialComponent.durability = EditorGUILayout.IntField ("Normal Durability : ", weaponMaterialComponent.durability);

		if (GUILayout.Button("Save Weapon Material")) {
			SaveWeaponMaterial(Utilities.NormalizeString(weaponMaterialComponent.material.ToString()));
		}
	}

	private void SaveWeaponMaterial(string fileName) {
		if (string.IsNullOrEmpty(fileName)) {
			EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
			return;
		}
		string path = Utilities.dataPath + "WeaponMaterials/" + fileName + ".json";
		if (Utilities.DoesFileExist(path)) {
			if (EditorUtility.DisplayDialog("Overwrite File", "A file with name " + fileName + " already exists. Replace with this file?", "Yes", "No")) {
				File.Delete(path);
				Save(path);
			}
		} else {
			Save(path);
		}
	}
	private void Save(string path){
		WeaponMaterial weaponMaterial = new WeaponMaterial ();
//			weaponMaterial.material = weaponMaterialComponent.material;
		weaponMaterial.power = weaponMaterialComponent.power;
		weaponMaterial.durability = weaponMaterialComponent.durability;

		string jsonString = JsonUtility.ToJson(weaponMaterial);

		System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
		writer.WriteLine(jsonString);
		writer.Close();
	}
}
#endif