#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ArmorMaterialComponent))]
public class ArmorMaterialCreator : Editor {

	ArmorMaterialComponent armorMaterialComponent;

	public override void OnInspectorGUI() {
		if(armorMaterialComponent == null){
			armorMaterialComponent = (ArmorMaterialComponent)target;
		}
		armorMaterialComponent.material = (MATERIAL)EditorGUILayout.EnumPopup ("Material : ", armorMaterialComponent.material);
		armorMaterialComponent.baseDamageMitigation = EditorGUILayout.FloatField ("Normal Base Damage Mitigation : ", armorMaterialComponent.baseDamageMitigation);
		armorMaterialComponent.damageNullificationChance = EditorGUILayout.FloatField ("Normal Damage Nullification : ", armorMaterialComponent.damageNullificationChance);
		armorMaterialComponent.durability = EditorGUILayout.IntField ("Normal Durability : ", armorMaterialComponent.durability);

		SerializedProperty ineffectiveAttackType = serializedObject.FindProperty("ineffectiveAttackTypes");
		EditorGUILayout.PropertyField(ineffectiveAttackType, true);
		serializedObject.ApplyModifiedProperties ();

		SerializedProperty effectiveAttackType = serializedObject.FindProperty("effectiveAttackTypes");
		EditorGUILayout.PropertyField(effectiveAttackType, true);
		serializedObject.ApplyModifiedProperties ();

		if (GUILayout.Button("Save Armor Material")) {
			SaveArmorMaterial(Utilities.NormalizeString(armorMaterialComponent.material.ToString()));
		}
	}

	private void SaveArmorMaterial(string fileName) {
		if (string.IsNullOrEmpty(fileName)) {
			EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
			return;
		}
        string path = Utilities.dataPath + "ArmorMaterials/" + fileName + ".json";
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
		ArmorMaterial armorMaterial = new ArmorMaterial ();
//			armorMaterial.material = armorMaterialComponent.material;
		armorMaterial.baseDamageMitigation = armorMaterialComponent.baseDamageMitigation;
		armorMaterial.damageNullificationChance = armorMaterialComponent.damageNullificationChance;
		armorMaterial.ineffectiveAttackTypes = armorMaterialComponent.ineffectiveAttackTypes;
		armorMaterial.effectiveAttackTypes = armorMaterialComponent.effectiveAttackTypes;
		armorMaterial.durability = armorMaterialComponent.durability;

		string jsonString = JsonUtility.ToJson(armorMaterial);

		System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
		writer.WriteLine(jsonString);
		writer.Close();
	}
}
#endif