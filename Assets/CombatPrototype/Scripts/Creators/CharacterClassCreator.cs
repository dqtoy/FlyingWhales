using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ECS {
    public class CharacterClassCreator : EditorWindow {

        private Vector2 scrollPos = Vector2.zero;

        private string className;
        public List<Skill> skills = new List<Skill>();
        private bool skillsFoldout;

        private SKILL_TYPE skillTypeToAdd;
        private int skillToAddIndex;

        // Add menu item to the Window menu
        [MenuItem("Window/Character Class Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(CharacterClassCreator));
        }

        private void OnGUI() {
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
            GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
            className = EditorGUILayout.TextField("Class Name: ", className);

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty skillProperty = serializedObject.FindProperty("skills");
            skillsFoldout = EditorGUILayout.Foldout(skillsFoldout, "Skills");

            if (skillsFoldout && skills != null) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < skills.Count; i++) {
                    SerializedProperty currSkill = skillProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(currSkill, true);
                }
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Add Skills ", EditorStyles.boldLabel);
            skillTypeToAdd = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type To Add: ", skillTypeToAdd);
            List<string> choices = GetAllSkillsOfType(skillTypeToAdd);
            skillToAddIndex = EditorGUILayout.Popup("Skill To Add: ", skillToAddIndex, choices.ToArray());
            if (GUILayout.Button("Add Skill")) {
                AddSkillToList(choices[skillToAddIndex]);
            }
            GUILayout.EndHorizontal();


            if (GUILayout.Button("Create Character Class")) {
                SaveCharacterClass();
            }
            if (GUILayout.Button("Load Character Class")) {
                LoadCharacterClass();
            }

            EditorGUILayout.EndScrollView();
        }

        #region Skills
        private List<string> GetAllSkillsOfType(SKILL_TYPE skillType) {
            List<string> allSkillsOfType = new List<string>();
            string path = "Assets/CombatPrototype/Data/Skills/" + skillType.ToString() + "/";
            foreach (string file in Directory.GetFiles(path, "*.json")) {
                allSkillsOfType.Add(Path.GetFileNameWithoutExtension(file));
            }
            return allSkillsOfType;
        }
        private void AddSkillToList(string skillName) {
            string path = "Assets/CombatPrototype/Data/Skills/" + skillTypeToAdd.ToString() + "/" + skillName + ".json";
            string dataAsJson = File.ReadAllText(path);
            switch (skillTypeToAdd) {
                case SKILL_TYPE.ATTACK:
                    AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
                    skills.Add(attackSkill);
                    break;
                case SKILL_TYPE.HEAL:
                    HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
                    skills.Add(healSkill);
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
                    skills.Add(obtainSkill);
                    break;
                case SKILL_TYPE.FLEE:
                    FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
                    skills.Add(fleeSkill);
                    break;
            }
        }
        #endregion

        #region Saving
        private void SaveCharacterClass() {
            if (string.IsNullOrEmpty(this.className)) {
                EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
                return;
            }
            string path = "Assets/CombatPrototype/Data/CharacterClasses/" + this.className + ".json";
            CharacterClass newClass = new CharacterClass();
            newClass.className = this.className;
            newClass.skills = this.skills.ToArray();
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Class", "A class with name " + this.className + " already exists. Replace with this class?", "Yes", "No")) {
                    File.Delete(path);
                    SaveCharacterClassJson(newClass, path);
                }
            } else {
                SaveCharacterClassJson(newClass, path);
            }
        }
        private void SaveCharacterClassJson(CharacterClass characterClass, string path) {
            string jsonString = JsonUtility.ToJson(characterClass);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved class" + characterClass.className + " at " + path);
        }
        #endregion

        #region Loading
        private void LoadCharacterClass() {
            string filePath = EditorUtility.OpenFilePanel("Select Character Json", "Assets/CombatPrototype/Data/CharacterClasses/", "json");
            if (!string.IsNullOrEmpty(filePath)) {
                string dataAsJson = File.ReadAllText(filePath);
                LoadCharacter(JsonUtility.FromJson<CharacterClass>(dataAsJson));
            }
        }
        private void LoadCharacter(CharacterClass character) {
            this.className = character.className;
            this.skills = new List<Skill>(character.skills);
        }
        #endregion
    }
}

