using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ECS {
    public class CharacterClassCreator : EditorWindow {

        private Vector2 scrollPos = Vector2.zero;

        [SerializeField] private CharacterClass currCharacterClass;

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
            if(currCharacterClass == null) {
                currCharacterClass = new CharacterClass();
            }
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
            GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
            currCharacterClass.className = EditorGUILayout.TextField("Class Name: ", currCharacterClass.className);
            currCharacterClass.actRate = EditorGUILayout.IntField("Act Rate: ", currCharacterClass.actRate);
            currCharacterClass.strGain = EditorGUILayout.IntField("Strength Gain: ", currCharacterClass.strGain);
            currCharacterClass.intGain = EditorGUILayout.IntField("Intelligence Gain: ", currCharacterClass.intGain);
            currCharacterClass.agiGain = EditorGUILayout.IntField("Agility Gain: ", currCharacterClass.agiGain);
            currCharacterClass.hpGain = EditorGUILayout.IntField("HP Gain: ", currCharacterClass.hpGain);
            currCharacterClass.dodgeRate = EditorGUILayout.IntField("Dodge Rate: ", currCharacterClass.dodgeRate);
            currCharacterClass.parryRate = EditorGUILayout.IntField("Parry Rate: ", currCharacterClass.parryRate);
            currCharacterClass.blockRate = EditorGUILayout.IntField("Block Rate: ", currCharacterClass.blockRate);

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty skillProperty = serializedObject.FindProperty("currCharacterClass");
            skillsFoldout = EditorGUILayout.Foldout(skillsFoldout, "Skills");

            if (skillsFoldout && currCharacterClass.skills != null) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currCharacterClass.skills.Count; i++) {
                    SerializedProperty currSkill = skillProperty.FindPropertyRelative("skills").GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(currSkill, true);
                }
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
            }

            //Add Skill Area
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Add Skills ", EditorStyles.boldLabel);
            skillTypeToAdd = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type To Add: ", skillTypeToAdd);
            List<string> choices = GetAllSkillsOfType(skillTypeToAdd);
            skillToAddIndex = EditorGUILayout.Popup("Skill To Add: ", skillToAddIndex, choices.ToArray());
            GUI.enabled = choices.Count > 0;
            if (GUILayout.Button("Add Skill")) {
                AddSkillToList(choices[skillToAddIndex]);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();


            if (GUILayout.Button("Create Character Class")) {
                SaveCharacterClass();
            }
            if (GUILayout.Button("Load Character Class")) {
                LoadCharacterClass();
            }
            if (GUILayout.Button("Reset Values")) {
                ResetValues();
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
                    currCharacterClass.skills.Add(attackSkill);
                    break;
                case SKILL_TYPE.HEAL:
                    HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
                    currCharacterClass.skills.Add(healSkill);
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
                    currCharacterClass.skills.Add(obtainSkill);
                    break;
                case SKILL_TYPE.FLEE:
                    FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
                    currCharacterClass.skills.Add(fleeSkill);
                    break;
                case SKILL_TYPE.MOVE:
                    MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
                    currCharacterClass.skills.Add(moveSkill);
                    break;
            }
        }
        #endregion

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
                ResetValues();
                string dataAsJson = File.ReadAllText(filePath);
                LoadCharacter(JsonUtility.FromJson<CharacterClass>(dataAsJson));
            }
        }
        private void LoadCharacter(CharacterClass character) {
            currCharacterClass = character;
        }
        #endregion

        private void ResetValues() {
            currCharacterClass = null;
        }
    }
}

