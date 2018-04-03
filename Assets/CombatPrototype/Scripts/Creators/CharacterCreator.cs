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
            GUILayout.Label("ECS.Character Setup Creator ", EditorStyles.boldLabel);
			characterComponent.fileName = EditorGUILayout.TextField("File Name: ", characterComponent.fileName);
            
			characterComponent.currRaceSelectedIndex = EditorGUILayout.Popup("Race Setup: ", characterComponent.currRaceSelectedIndex, characterComponent.raceChoices.ToArray());
			characterComponent.currCharacterSelectedIndex = EditorGUILayout.Popup("ECS.Character Class: ", characterComponent.currCharacterSelectedIndex, characterComponent.characterClassChoices.ToArray());
			characterComponent.optionalRole = (CHARACTER_ROLE)EditorGUILayout.EnumPopup("Optional Role: ", characterComponent.optionalRole);

            SerializedProperty serializedTags = serializedObject.FindProperty("tags");
            EditorGUILayout.PropertyField(serializedTags, true);
            serializedObject.ApplyModifiedProperties();

            //			SerializedProperty serializedProperty = serializedObject.FindProperty("preEquippedItems");
            //			EditorGUILayout.PropertyField(serializedProperty, true);
            //			serializedObject.ApplyModifiedProperties();

            characterComponent.itemFoldout = EditorGUILayout.Foldout(characterComponent.itemFoldout, "Pre-equipped Items");

			if (characterComponent.itemFoldout && characterComponent.preEquippedItems != null) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < characterComponent.preEquippedItems.Count; i++) {
					SerializedProperty currItem = serializedObject.FindProperty("preEquippedItems").GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(currItem, true);
//					EditorGUILayout.LabelField(characterComponent.preEquippedItems[i]);
//					if (GUILayout.Button("Remove")) {
//						characterComponent.RemoveItem(i);
//					}
				}
				EditorGUI.indentLevel--;
			}


			//Add Item Area
			GUILayout.Space(10);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.Label("Add Items ", EditorStyles.boldLabel);
			characterComponent.itemTypeToAdd = (ITEM_TYPE)EditorGUILayout.EnumPopup("Item Type To Add: ", characterComponent.itemTypeToAdd);
			List<string> choices = GetAllItemsOfType(characterComponent.itemTypeToAdd);
			characterComponent.currItemSelectedIndex = EditorGUILayout.Popup("Item To Add: ", characterComponent.currItemSelectedIndex, choices.ToArray());
			GUI.enabled = choices.Count > 0;
			if (GUILayout.Button("Add Item")) {
				characterComponent.AddItem(choices[characterComponent.currItemSelectedIndex]);
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();



			characterComponent.raceSettingName = characterComponent.raceChoices[characterComponent.currRaceSelectedIndex];
			characterComponent.characterClassName = characterComponent.characterClassChoices[characterComponent.currCharacterSelectedIndex];

			if (GUILayout.Button("Save ECS.Character")) {
				SaveCharacter(ConstructCharacterSetup());
			}
        }

        #region ECS.Character Classes
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
				if (EditorUtility.DisplayDialog("Overwrite ECS.Character", characterComponent.fileName + " already exists. Replace with this character?", "Yes", "No")) {
                    File.Delete(path);
                    SaveCharacterJson(path, characterSetup);
                }
            } else {
                SaveCharacterJson(path, characterSetup);
            }
        }
		private CharacterSetup ConstructCharacterSetup() {
            CharacterSetup newCharacter = new CharacterSetup();
            //string raceData = File.ReadAllText("Assets/CombatPrototype/Data/RaceSettings/" + raceSettingFileName + ".json");
            //string characterClassData = File.ReadAllText("Assets/CombatPrototype/Data/CharacterClasses/" + characterClassFileName + ".json");
            newCharacter.fileName = characterComponent.fileName;
			newCharacter.raceSettingName = characterComponent.raceSettingName;
			newCharacter.characterClassName = characterComponent.characterClassName;
			newCharacter.optionalRole = characterComponent.optionalRole;
            newCharacter.tags = characterComponent.tags;
            newCharacter.preEquippedItems = characterComponent.preEquippedItems;

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

		private List<string> GetAllItems() {
			string mainPath = "Assets/CombatPrototype/Data/Items/";
			string[] folders = System.IO.Directory.GetDirectories (mainPath);
			List<string> allItemsOfType = new List<string>();
			for (int i = 0; i < folders.Length; i++) {
				string path = folders[i] + "/";
				foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
					allItemsOfType.Add(System.IO.Path.GetFileNameWithoutExtension(file));
				}
			} 

			return allItemsOfType;
		}
		private List<string> GetAllItemsOfType(ITEM_TYPE itemType) {
			List<string> allItemsOfType = new List<string>();
			string path = "Assets/CombatPrototype/Data/Items/" + itemType.ToString() + "/";
			foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
				allItemsOfType.Add(System.IO.Path.GetFileNameWithoutExtension(file));
			}
			return allItemsOfType;
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

