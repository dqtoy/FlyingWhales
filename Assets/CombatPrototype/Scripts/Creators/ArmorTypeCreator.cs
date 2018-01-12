using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	[CustomEditor(typeof(ArmorTypeComponent))]
	public class ArmorTypeCreator : Editor {

		ArmorTypeComponent armorTypeComponent;

		public override void OnInspectorGUI() {
			if(armorTypeComponent == null){
				armorTypeComponent = (ArmorTypeComponent)target;
			}
			armorTypeComponent.armorType = (ARMOR_TYPE)EditorGUILayout.EnumPopup ("Armor Type: ", armorTypeComponent.armorType);
			armorTypeComponent.armorBodyType = (BODY_PART)EditorGUILayout.EnumPopup ("Armor Body Type: ", armorTypeComponent.armorBodyType);

			if (GUILayout.Button("Save Armor Type")) {
				SaveArmorType(Utilities.NormalizeString(armorTypeComponent.armorType.ToString()));
			}
		}

		private void SaveArmorType(string fileName) {
			if (string.IsNullOrEmpty(fileName)) {
				EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
				return;
			}
			string path = "Assets/CombatPrototype/Data/ArmorTypes/" + fileName + ".json";
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
			ArmorType armorType = new ArmorType ();
			armorType.armorType = armorTypeComponent.armorType;
			armorType.armorBodyType = armorTypeComponent.armorBodyType;

			string jsonString = JsonUtility.ToJson(armorType);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();
		}
	}
}
