using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ECS {
    public class BodyPartsCreator : EditorWindow {
		public BodyPartsData bodyPartsData;

   //     // Add menu item to the Window menu
   //     [MenuItem("Window/Body Parts Creator")]
   //     public static void ShowWindow() {
   //         //Show existing window instance. If one doesn't exist, make one.
			//EditorWindow.GetWindow(typeof(BodyPartsCreator));
   //     }

        void OnGUI() {
            GUILayout.Label("Body Parts Creator ", EditorStyles.boldLabel);

			if(bodyPartsData != null){
				SerializedObject serializedObject = new SerializedObject (this);
				SerializedProperty serializedProperty = serializedObject.FindProperty ("bodyPartsData");
				EditorGUILayout.PropertyField (serializedProperty, true);
				serializedObject.ApplyModifiedProperties ();
			}else{
				bodyPartsData = new BodyPartsData();
			}

			if (GUILayout.Button("Save Body Parts")) {
				SaveBodyParts();
			}
			if (GUILayout.Button("Load Body Parts")) {
				LoadBodyParts();
			}
			if (GUILayout.Button("New Body Parts")) {
				NewBodyParts();
			}
        }

		private void SaveBodyParts(){
			string fileName = bodyPartsData.race.ToString() + "_BODY_PARTS";
			string path = "Assets/CombatPrototype/Data/BodyPartsTemplate/" + fileName + ".json";
			if (File.Exists(path)) {
				if (EditorUtility.DisplayDialog("Overwrite Body Parts Template", fileName + " already exists. Replace with this new template?", "Yes", "No")) {
					File.Delete(path);
					SaveBodyPartsJson(path);
				}
			} else {
				SaveBodyPartsJson(path);
			}
		}
		private void SaveBodyPartsJson(string path) {
			string jsonString = JsonUtility.ToJson(bodyPartsData);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();

			//Re-import the file to update the reference in the editor
			UnityEditor.AssetDatabase.ImportAsset(path);
			Debug.Log("Successfully saved body parts data at " + path);
		}
		private void LoadBodyParts(){
			string filePath = EditorUtility.OpenFilePanel ("Select body parts data file", "Assets/CombatPrototype/Data/BodyPartsTemplate/" , "json");

			if (!string.IsNullOrEmpty (filePath)) 
			{
				string dataAsJson = File.ReadAllText (filePath);

				bodyPartsData = JsonUtility.FromJson<BodyPartsData> (dataAsJson);
			}
		}

		private void NewBodyParts(){
			bodyPartsData = new BodyPartsData ();
		}
    }
}

