using UnityEngine;
using System.Collections;
using UnityEditor;
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
			currCharacterClass.strPercentage = EditorGUILayout.FloatField("Strength Percentage: ", currCharacterClass.strPercentage);
			currCharacterClass.intPercentage = EditorGUILayout.FloatField("Intelligence Percentage: ", currCharacterClass.intPercentage);
			currCharacterClass.agiPercentage = EditorGUILayout.FloatField("Agility Percentage: ", currCharacterClass.agiPercentage);
			currCharacterClass.hpPercentage = EditorGUILayout.FloatField("HP Percentage: ", currCharacterClass.hpPercentage);
            currCharacterClass.dodgeRate = EditorGUILayout.IntField("Dodge Rate: ", currCharacterClass.dodgeRate);
            currCharacterClass.parryRate = EditorGUILayout.IntField("Parry Rate: ", currCharacterClass.parryRate);
            currCharacterClass.blockRate = EditorGUILayout.IntField("Block Rate: ", currCharacterClass.blockRate);


            if (GUILayout.Button("Create ECS.Character Class")) {
                SaveCharacterClass();
            }
        }

//        #region Skills
//        private List<string> GetAllSkillsOfType(SKILL_TYPE skillType) {
//            List<string> allSkillsOfType = new List<string>();
//            string path = "Assets/CombatPrototype/Data/Skills/" + skillType.ToString() + "/";
//            foreach (string file in Directory.GetFiles(path, "*.json")) {
//                allSkillsOfType.Add(Path.GetFileNameWithoutExtension(file));
//            }
//            return allSkillsOfType;
//        }
//        private void AddSkillToList(string skillName) {
//			string path = "Assets/CombatPrototype/Data/Skills/" + currCharacterClass.skillTypeToAdd.ToString() + "/" + skillName + ".json";
//            string dataAsJson = File.ReadAllText(path);
//			switch (currCharacterClass.skillTypeToAdd) {
//            case SKILL_TYPE.ATTACK:
//                AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
//				currCharacterClass.AddSkillOfType(currCharacterClass.skillTypeToAdd, attackSkill);
//                break;
//            case SKILL_TYPE.HEAL:
//                HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
//				currCharacterClass.AddSkillOfType(currCharacterClass.skillTypeToAdd, healSkill);
//                break;
//            case SKILL_TYPE.OBTAIN_ITEM:
//                ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
//				currCharacterClass.AddSkillOfType(currCharacterClass.skillTypeToAdd, obtainSkill);
//                break;
//            case SKILL_TYPE.FLEE:
//                FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
//				currCharacterClass.AddSkillOfType(currCharacterClass.skillTypeToAdd, fleeSkill);
//                break;
//            case SKILL_TYPE.MOVE:
//                MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
//				currCharacterClass.AddSkillOfType(currCharacterClass.skillTypeToAdd, moveSkill);
//                break;
//            }
//        }
//        #endregion

        #region Saving
        private void SaveCharacterClass() {
            if (string.IsNullOrEmpty(currCharacterClass.className)) {
                EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
                return;
            }
            string path = "Assets/CombatPrototype/Data/CharacterClasses/" + currCharacterClass.className + ".json";
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
            string jsonString = JsonUtility.ToJson(characterClass);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved class " + characterClass.className + " at " + path);
        }
        #endregion

        #region Loading
//        private void LoadCharacterClass() {
//            string filePath = EditorUtility.OpenFilePanel("Select ECS.Character Json", "Assets/CombatPrototype/Data/CharacterClasses/", "json");
//            if (!string.IsNullOrEmpty(filePath)) {
//                ResetValues();
//                string dataAsJson = File.ReadAllText(filePath);
//                LoadCharacter(JsonUtility.FromJson<CharacterClass>(dataAsJson));
//            }
//        }
//        private void LoadCharacter(CharacterClass character) {
//            currCharacterClass = character;
//            currCharacterClass.ConstructAllSkillsList();
//        }
        #endregion

        private void ResetValues() {
            currCharacterClass = null;
        }
    }
}

