#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ECS {
	[CustomEditor(typeof(ClassComponent))]
    public class CharacterClassCreator : Editor {

		ClassComponent currCharacterClass;

        public override void OnInspectorGUI() {
			if(currCharacterClass == null){
				currCharacterClass = (ClassComponent)target;
			}
            GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
            currCharacterClass.className = EditorGUILayout.TextField("Class Name: ", currCharacterClass.className);
            currCharacterClass.strWeightAllocation = EditorGUILayout.FloatField("Strength Weight Allocation: ", currCharacterClass.strWeightAllocation);
            currCharacterClass.intWeightAllocation = EditorGUILayout.FloatField("Intelligence Weight Allocation: ", currCharacterClass.intWeightAllocation);
            currCharacterClass.agiWeightAllocation = EditorGUILayout.FloatField("Agility Weight Allocation: ", currCharacterClass.agiWeightAllocation);
            currCharacterClass.vitWeightAllocation = EditorGUILayout.FloatField("Vitality Weight Allocation: ", currCharacterClass.vitWeightAllocation);
            //currCharacterClass.dodgeRate = EditorGUILayout.IntField("Dodge Rate: ", currCharacterClass.dodgeRate);
            //currCharacterClass.parryRate = EditorGUILayout.IntField("Parry Rate: ", currCharacterClass.parryRate);
            //currCharacterClass.blockRate = EditorGUILayout.IntField("Block Rate: ", currCharacterClass.blockRate);

            SerializedProperty allowedWeaponType = serializedObject.FindProperty("allowedWeaponTypes");
			EditorGUILayout.PropertyField(allowedWeaponType, true);
			serializedObject.ApplyModifiedProperties ();

            SerializedProperty skillsPerLevel = serializedObject.FindProperty("skillsPerLevel");
            EditorGUILayout.PropertyField(skillsPerLevel, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Create ECS.Character Class")) {
                SaveCharacterClass();
            }
        }

        #region Saving
        private void SaveCharacterClass() {
            if (string.IsNullOrEmpty(currCharacterClass.className)) {
                EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
                return;
            }
            string path = Utilities.dataPath + "CharacterClasses/" + currCharacterClass.className + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Class", "A class with name " + currCharacterClass.className + " already exists. Replace with this class?", "Yes", "No")) {
                    File.Delete(path);
                    SaveCharacterClassJson(currCharacterClass, path);
                }
            } else {
                SaveCharacterClassJson(currCharacterClass, path);
            }
        }
        private void SaveCharacterClassJson(ClassComponent characterClass, string path) {
            if(characterClass.skillsPerLevelNames == null) {
                characterClass.skillsPerLevelNames = new List<string[]>();
            } else {
                characterClass.skillsPerLevelNames.Clear();
            }
            for (int i = 0; i < characterClass.skillsPerLevel.Count; i++) {
                string[] skillNames = new string[characterClass.skillsPerLevel[i].list.Count];
                for (int j = 0; j < characterClass.skillsPerLevel[i].list.Count; j++) {
                    skillNames[j] = characterClass.skillsPerLevel[i].list[j].name;
                }
                characterClass.skillsPerLevelNames.Add(skillNames);
            }
            string jsonString = JsonUtility.ToJson(characterClass);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved class " + characterClass.className + " at " + path);
        }
        #endregion

        private void ResetValues() {
            currCharacterClass = null;
        }
    }
}
#endif