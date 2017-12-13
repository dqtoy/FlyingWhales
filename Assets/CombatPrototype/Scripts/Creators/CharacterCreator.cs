using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ECS {
    public class CharacterCreator : EditorWindow {

        public string fileName;

        private int characterClassIndex;
        private int raceSetupIndex;

        List<string> raceChoices;
        List<string> characterClassChoices;

        // Add menu item to the Window menu
        [MenuItem("Window/Character Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(CharacterCreator));
        }

        private void OnGUI() {
            GUILayout.Label("Character Setup Creator ", EditorStyles.boldLabel);
            fileName = EditorGUILayout.TextField("File Name: ", fileName);
            
            raceChoices = GetAllRaceSetups();
            raceSetupIndex = EditorGUILayout.Popup("Race Setups: ", raceSetupIndex, raceChoices.ToArray());

            characterClassChoices = GetAllCharacterClasses();
            characterClassIndex = EditorGUILayout.Popup("Character Class: ", characterClassIndex, characterClassChoices.ToArray());

            if (GUILayout.Button("Save Character")) {
                SaveCharacter(ConstructCharacterSetup(raceChoices[raceSetupIndex], characterClassChoices[characterClassIndex]));
            }
            if (GUILayout.Button("Load Character")) {
                LoadCharacter();
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
            string path = "Assets/CombatPrototype/Data/CharacterSetups/" + fileName + ".json";
            if (File.Exists(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Character", fileName + " already exists. Replace with this character?", "Yes", "No")) {
                    File.Delete(path);
                    SaveCharacterJson(path, characterSetup);
                }
            } else {
                SaveCharacterJson(path, characterSetup);
            }
        }
        private CharacterSetup ConstructCharacterSetup(string raceSettingFileName, string characterClassFileName) {
            CharacterSetup newCharacter = new CharacterSetup();
            string raceData = File.ReadAllText("Assets/CombatPrototype/Data/RaceSettings/" + raceSettingFileName + ".json");
            string characterClassData = File.ReadAllText("Assets/CombatPrototype/Data/CharacterClasses/" + characterClassFileName + ".json");
            newCharacter.fileName = fileName;
            newCharacter.raceSetting = JsonUtility.FromJson<RaceSetting>(raceData);
            newCharacter.characterClass = JsonUtility.FromJson<CharacterClass>(characterClassData);

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
        private void LoadCharacter() {
            string filePath = EditorUtility.OpenFilePanel("Select character data file", "Assets/CombatPrototype/Data/CharacterSetups/", "json");

            if (!string.IsNullOrEmpty(filePath)) {
                string dataAsJson = File.ReadAllText(filePath);

                CharacterSetup characterSetup = JsonUtility.FromJson<CharacterSetup>(dataAsJson);

                this.fileName = characterSetup.fileName;
                for (int i = 0; i < raceChoices.Count; i++) {
                    string currChoice = raceChoices[i];
                    if (currChoice.Equals(characterSetup.raceSetting.race.ToString())) {
                        raceSetupIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < characterClassChoices.Count; i++) {
                    string currChoice = characterClassChoices[i];
                    if (currChoice.Equals(characterSetup.characterClass.className.ToString())) {
                        characterClassIndex = i;
                        break;
                    }
                }
            }
        }
    }
}

