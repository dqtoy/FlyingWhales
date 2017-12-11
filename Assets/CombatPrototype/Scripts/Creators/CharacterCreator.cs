using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace ECS {
    public class CharacterCreator : EditorWindow {

        public string fileName;
		public Character character;
        // Add menu item to the Window menu
        [MenuItem("Window/Character Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(CharacterCreator));
        }

        private void OnGUI() {
            GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
			fileName = EditorGUILayout.TextField("File Name: ", fileName);

			if(character != null){
				SerializedObject serializedObject = new SerializedObject (this);
				SerializedProperty serializedProperty = serializedObject.FindProperty ("character");
				EditorGUILayout.PropertyField (serializedProperty, true);
				serializedObject.ApplyModifiedProperties ();
			}else{
				character = new Character();
			}

			if (GUILayout.Button("Save Character")) {
				SaveCharacter();
			}
			if (GUILayout.Button("Load Character")) {
				LoadCharacter();
			}
			if (GUILayout.Button("New Character")) {
				NewCharacter();
			}
        }

		private void SaveCharacter(){
			string path = "Assets/CombatPrototype/Data/Characters/" + fileName + ".json";
			if (File.Exists(path)) {
				if (EditorUtility.DisplayDialog("Overwrite Character", fileName + " already exists. Replace with this new character?", "Yes", "No")) {
					File.Delete (path);
					SaveCharacterJson(path);
				}
			} else {
				SaveCharacterJson(path);
			}
		}
		private void SaveCharacterJson(string path) {
			string jsonString = JsonUtility.ToJson(character);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();

			//Re-import the file to update the reference in the editor
			UnityEditor.AssetDatabase.ImportAsset(path);
			Debug.Log("Successfully saved character data at " + path);
		}
		private void LoadCharacter(){
			string filePath = EditorUtility.OpenFilePanel ("Select character data file", "Assets/CombatPrototype/Data/Characters/" , "json");

			if (!string.IsNullOrEmpty (filePath)) 
			{
				this.fileName = Path.GetFileNameWithoutExtension (filePath);
				string dataAsJson = File.ReadAllText (filePath);

				character = JsonUtility.FromJson<Character> (dataAsJson);
			}
		}

		private void NewCharacter(){
			this.fileName = string.Empty;
			character = new Character ();
		}
    }
}

