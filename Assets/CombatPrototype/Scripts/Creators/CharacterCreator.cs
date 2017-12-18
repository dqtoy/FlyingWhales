using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ECS {
	[CustomEditor(typeof(CharacterComponent))]
    public class CharacterCreator : Editor {

		CharacterComponent characterComponent;

		public override void OnInspectorGUI() {
			if(characterComponent == null){
				characterComponent = (CharacterComponent)target;
			}
            GUILayout.Label("Character Setup Creator ", EditorStyles.boldLabel);
			characterComponent.fileName = EditorGUILayout.TextField("File Name: ", characterComponent.fileName);
            
//			characterComponent.raceChoices = GetAllRaceSetups();
			characterComponent.raceSetup = (TextAsset)EditorGUILayout.ObjectField("Race Setup: ", characterComponent.raceSetup, typeof(TextAsset), false);
//
//			characterComponent.characterClassChoices = GetAllCharacterClasses();
			characterComponent.characterClass = (TextAsset)EditorGUILayout.ObjectField("Character Class: ", characterComponent.characterClass, typeof(TextAsset), false);

			if(characterComponent.raceSetup != null && characterComponent.characterClass != null){
				if (GUILayout.Button("Save Character")) {
					SaveCharacter(ConstructCharacterSetup(characterComponent.raceSetup, characterComponent.characterClass));
				}
			}
        }

        #region Character Classes
        private List<string> GetAllCharacterClasses() {
            List<string> allCharacterClasses = new List<string>();
            string path = "Assets/CombatPrototype/Data/CharacterClasses/";
            foreach (string file in Directory.GetFiles(path, "*.json")) {
                allCharacterClasses.Add(Path.GetFileNameWithoutExtension(file));
            }
            return allCharacterClasses;
        }
        #endregion

        #region Race Setups
        private List<string> GetAllRaceSetups() {
            List<string> allRaceSetups = new List<string>();
            string path = "Assets/CombatPrototype/Data/RaceSettings/";
            foreach (string file in Directory.GetFiles(path, "*.json")) {
                allRaceSetups.Add(Path.GetFileNameWithoutExtension(file));
            }
            return allRaceSetups;
        }
        #endregion

        private void SaveCharacter(CharacterSetup characterSetup) {
			string path = "Assets/CombatPrototype/Data/CharacterSetups/" + characterComponent.fileName + ".json";
            if (File.Exists(path)) {
				if (EditorUtility.DisplayDialog("Overwrite Character", characterComponent.fileName + " already exists. Replace with this character?", "Yes", "No")) {
                    File.Delete(path);
                    SaveCharacterJson(path, characterSetup);
                }
            } else {
                SaveCharacterJson(path, characterSetup);
            }
        }
        private CharacterSetup ConstructCharacterSetup(TextAsset raceSettingJson, TextAsset characterClassJson) {
            CharacterSetup newCharacter = new CharacterSetup();
            //string raceData = File.ReadAllText("Assets/CombatPrototype/Data/RaceSettings/" + raceSettingFileName + ".json");
            //string characterClassData = File.ReadAllText("Assets/CombatPrototype/Data/CharacterClasses/" + characterClassFileName + ".json");
            newCharacter.fileName = characterComponent.fileName;
			newCharacter.raceSetupJson = raceSettingJson;
			newCharacter.characterClassJson = characterClassJson;

            return newCharacter;
        }
        private void SaveCharacterJson(string path, CharacterSetup character) {
            string jsonString = JsonUtility.ToJson(character);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved character data at " + path);
        }
//        private void LoadCharacter() {
//            string filePath = EditorUtility.OpenFilePanel("Select character data file", "Assets/CombatPrototype/Data/CharacterSetups/", "json");
//
//            if (!string.IsNullOrEmpty(filePath)) {
//                string dataAsJson = File.ReadAllText(filePath);
//
//                CharacterSetup characterSetup = JsonUtility.FromJson<CharacterSetup>(dataAsJson);
//
//                this.fileName = characterSetup.fileName;
//                for (int i = 0; i < raceChoices.Count; i++) {
//                    string currChoice = raceChoices[i];
//                    if (currChoice.Equals(characterSetup.raceSettingName)) {
//                        raceSetupIndex = i;
//                        break;
//                    }
//                }
//
//                for (int i = 0; i < characterClassChoices.Count; i++) {
//                    string currChoice = characterClassChoices[i];
//                    if (currChoice.Equals(characterSetup.characterClassName)) {
//                        characterClassIndex = i;
//                        break;
//                    }
//                }
//            }
//        }
    }
}

