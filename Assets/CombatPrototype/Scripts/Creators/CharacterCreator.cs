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
				characterComponent.raceChoices = GetAllRaceSetups();
				characterComponent.characterClassChoices = GetAllCharacterClasses();

				if(characterComponent.raceSettingName != string.Empty){
					for (int i = 0; i < characterComponent.raceChoices.Count; i++) {
						if(characterComponent.raceSettingName == characterComponent.raceChoices[i]){
							characterComponent.currRaceSelectedIndex = i;
							break;
						}
					}
				}

				if(characterComponent.characterClassName != string.Empty){
					for (int i = 0; i < characterComponent.characterClassChoices.Count; i++) {
						if(characterComponent.characterClassName == characterComponent.characterClassChoices[i]){
							characterComponent.currCharacterSelectedIndex = i;
							break;
						}
					}
				}
			}
            GUILayout.Label("Character Setup Creator ", EditorStyles.boldLabel);
			characterComponent.fileName = EditorGUILayout.TextField("File Name: ", characterComponent.fileName);
            
			characterComponent.currRaceSelectedIndex = EditorGUILayout.Popup("Race Setup: ", characterComponent.currRaceSelectedIndex, characterComponent.raceChoices.ToArray());
			characterComponent.currCharacterSelectedIndex = EditorGUILayout.Popup("Character Class: ", characterComponent.currCharacterSelectedIndex, characterComponent.characterClassChoices.ToArray());
			characterComponent.raceSettingName = characterComponent.raceChoices[characterComponent.currRaceSelectedIndex];
			characterComponent.characterClassName = characterComponent.characterClassChoices[characterComponent.currCharacterSelectedIndex];

			if (GUILayout.Button("Save Character")) {
				SaveCharacter(ConstructCharacterSetup(characterComponent.raceSettingName, characterComponent.characterClassName));
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
		private CharacterSetup ConstructCharacterSetup(string raceSettingName, string characterClassName) {
            CharacterSetup newCharacter = new CharacterSetup();
            //string raceData = File.ReadAllText("Assets/CombatPrototype/Data/RaceSettings/" + raceSettingFileName + ".json");
            //string characterClassData = File.ReadAllText("Assets/CombatPrototype/Data/CharacterClasses/" + characterClassFileName + ".json");
            newCharacter.fileName = characterComponent.fileName;
			newCharacter.raceSettingName = raceSettingName;
			newCharacter.characterClassName = characterClassName;

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

