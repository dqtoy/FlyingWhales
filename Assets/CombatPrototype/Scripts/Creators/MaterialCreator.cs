#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MaterialComponent))]
public class MaterialCreator : Editor {

	MaterialComponent materialComponent;

	public override void OnInspectorGUI() {
		if(materialComponent == null){
			materialComponent = (MaterialComponent)target;
		}
		materialComponent.material = (MATERIAL)EditorGUILayout.EnumPopup ("Material : ", materialComponent.material);
		materialComponent.category = (MATERIAL_CATEGORY)EditorGUILayout.EnumPopup ("Category : ", materialComponent.category);
//		materialComponent.technology = (TECHNOLOGY)EditorGUILayout.EnumPopup ("Technology : ", materialComponent.technology);

		materialComponent.weight = EditorGUILayout.IntField ("Weight : ", materialComponent.weight);
		materialComponent.isEdible = EditorGUILayout.Toggle ("Is Edible : ", materialComponent.isEdible);

		SerializedProperty structure = serializedObject.FindProperty("structure");
		EditorGUILayout.PropertyField(structure, true);
		serializedObject.ApplyModifiedProperties ();

		//Weapon Data Area
		GUILayout.Space(10);
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("Weapon Data ", EditorStyles.boldLabel);
		SerializedProperty weaponData = serializedObject.FindProperty("weaponData");
		EditorGUILayout.PropertyField(weaponData, true);
		serializedObject.ApplyModifiedProperties ();
//		GUI.enabled = true;
		GUILayout.EndHorizontal();

		//Armor Data Area
		GUILayout.Space(10);
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("Armor Data ", EditorStyles.boldLabel);
		SerializedProperty armorData = serializedObject.FindProperty("armorData");
		EditorGUILayout.PropertyField(armorData, true);
		serializedObject.ApplyModifiedProperties ();
//		GUI.enabled = true;
		GUILayout.EndHorizontal();

		//Construction Data Area
		GUILayout.Space(10);
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("Construction Data ", EditorStyles.boldLabel);
		materialComponent.sturdiness = EditorGUILayout.IntField ("Sturdiness : ", materialComponent.sturdiness);
		GUILayout.EndHorizontal();

		//Training Data Area
		GUILayout.Space(10);
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("Training Data ", EditorStyles.boldLabel);
		materialComponent.trainingStatBonus = EditorGUILayout.IntField ("Training Stat Bonus : ", materialComponent.trainingStatBonus);
		GUILayout.EndHorizontal();

		GUI.enabled = true;

		if (GUILayout.Button("Save Material")) {
			SaveMaterial(Utilities.NormalizeString(materialComponent.material.ToString()));
		}
	}

	private void SaveMaterial(string fileName) {
		if (string.IsNullOrEmpty(fileName)) {
			EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
			return;
		}
		string path = Utilities.dataPath + "Materials/" + fileName + ".json";
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
		Materials material = new Materials ();
		material.material = materialComponent.material;
		material.category = materialComponent.category;
//		material.technology = materialComponent.technology;
		material.weight = materialComponent.weight;
		material.isEdible = materialComponent.isEdible;
		//material.structure = materialComponent.structure;
		material.weaponData = materialComponent.weaponData;
		material.armorData = materialComponent.armorData;
		material.sturdiness = materialComponent.sturdiness;
		material.trainingStatBonus = materialComponent.trainingStatBonus;

		string jsonString = JsonUtility.ToJson(material);

		System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
		writer.WriteLine(jsonString);
		writer.Close();
	}
}
#endif