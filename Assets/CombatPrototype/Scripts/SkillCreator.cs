using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace ECS {
    public class SkillCreator : EditorWindow {

        private SKILL_TYPE skillType;
        private string skillName;
        private int activationWeight;
        private float accuracy;
        public SkillRequirement[] skillRequirements;

        //Attack Skill Fields
        private int attackPower;
        private ATTACK_TYPE attackType;
        private STATUS_EFFECT statusEffect;
        private int statusEffectRate;
        private int injuryRate;
        private int decapitationRate;

        // Add menu item to the Window menu
        [MenuItem("Window/Skill Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(SkillCreator));
        }

        private void OnGUI() {
            GUILayout.Label("Skill Creator ", EditorStyles.boldLabel);
            skillType = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type: ", skillType);
            skillName = EditorGUILayout.TextField("Skill Name: ", skillName);
            activationWeight = EditorGUILayout.IntField("Activation Weight: ", activationWeight);
            accuracy = EditorGUILayout.Slider("Accuracy: ", accuracy, 0f, 100f);

            switch (skillType) {
                case SKILL_TYPE.ATTACK:
                    ShowAttackSkillFields();
                    break;
                case SKILL_TYPE.HEAL:
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    break;
                case SKILL_TYPE.FLEE:
                    break;
            }

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("skillRequirements");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save Skill")) {
                SaveSkill(skillName);
            }
        }

        private void ShowAttackSkillFields() {
            attackPower = EditorGUILayout.IntField("Attack Power: ", attackPower);
            attackType = (ATTACK_TYPE)EditorGUILayout.EnumPopup("Attack Type: ", attackType);
            statusEffect = (STATUS_EFFECT)EditorGUILayout.EnumPopup("Status Effect: ", statusEffect);
            
        }

        #region Saving
        private void SaveSkill(string fileName) {
            string path = "Assets/CombatPrototype/Data/Skills/" + fileName + ".json";
            if (DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Skill", "A skil with name " + fileName + " already exists. Replace with this new skill?", "Yes", "No")) {
                    File.Delete(path);
                    SaveSkillJson(path);
                }
            } else {
                SaveSkillJson(path);
            }
        }
        private void SaveSkillJson(string path) {
            Skill newSkill = new Skill();
            newSkill.skillName = this.skillName;
            newSkill.activationWeight = this.activationWeight;
            newSkill.accuracy = this.accuracy;
            newSkill.skillRequirements = this.skillRequirements;

            string jsonString = JsonUtility.ToJson(newSkill);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved skill at " + path);
        }
        private bool DoesFileExist(string path) {
            return File.Exists(path);
        }
        #endregion
    }
}

