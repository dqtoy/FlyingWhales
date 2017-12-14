using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace ECS {
    public class SkillCreator : EditorWindow {

        private Vector2 scrollPos = Vector2.zero;

        private SKILL_TYPE skillType;
        private string skillName;
        private int activationWeight;
        private float accuracy;
        private int range;
        public SkillRequirement[] skillRequirements;
        public CHARACTER_ATTRIBUTES attributeModifier;

        //Attack Skill Fields
        private int attackPower;
        private ATTACK_TYPE attackType;
        private STATUS_EFFECT statusEffect;
        private int statusEffectRate;
        private int injuryRate;
        private int decapitationRate;

        //Heal Skill Fields
        private int healPower;

        // Add menu item to the Window menu
        [MenuItem("Window/Skill Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(SkillCreator));
        }

        private void OnGUI() {
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
            GUILayout.Label("Skill Creator ", EditorStyles.boldLabel);
            skillType = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type: ", skillType);
            skillName = EditorGUILayout.TextField("Skill Name: ", skillName);
            activationWeight = EditorGUILayout.IntField("Activation Weight: ", activationWeight);
            range = EditorGUILayout.IntField("Range: ", range);
            accuracy = EditorGUILayout.Slider("Accuracy: ", accuracy, 0f, 100f);
            attributeModifier = (CHARACTER_ATTRIBUTES)EditorGUILayout.EnumPopup("Attribute Modifier: ", attributeModifier);
            switch (skillType) {
                case SKILL_TYPE.ATTACK:
                    ShowAttackSkillFields();
                    break;
                case SKILL_TYPE.HEAL:
                    ShowHealSkillFields();
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    ShowObtainItemFields();
                    break;
                case SKILL_TYPE.FLEE:
                    ShowFleeItemFields();
                    break;
                case SKILL_TYPE.MOVE:
                    ShowMoveItemFields();
                    break;
            }

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("skillRequirements");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save Skill")) {
                SaveSkill(skillName);
            }

            if (GUILayout.Button("Load Skill")) {
                LoadSkill();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowAttackSkillFields() {
            attackPower = EditorGUILayout.IntField("Attack Power: ", attackPower);
            attackType = (ATTACK_TYPE)EditorGUILayout.EnumPopup("Attack Type: ", attackType);
            statusEffect = (STATUS_EFFECT)EditorGUILayout.EnumPopup("Status Effect: ", statusEffect);
            if(statusEffect != STATUS_EFFECT.NONE) {
                EditorGUI.indentLevel++;
                statusEffectRate = EditorGUILayout.IntField("Status Effect Rate: ", statusEffectRate);
                EditorGUI.indentLevel--;
            }
            injuryRate = EditorGUILayout.IntField("Injury Rate: ", injuryRate);
            decapitationRate = EditorGUILayout.IntField("Decapitation Rate: ", decapitationRate);
        }
        private void ShowHealSkillFields() {
            healPower = EditorGUILayout.IntField("Heal Power: ", healPower);
        }
        private void ShowObtainItemFields() {
            //Nothing yet
        }
        private void ShowFleeItemFields() {
            //Nothing yet
        }
        private void ShowMoveItemFields() {
            //Nothing yet
        }

        #region Saving
        private void SaveSkill(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                EditorUtility.DisplayDialog("Error", "Please specify a Skill Name", "OK");
                return;
            }
            string path = "Assets/CombatPrototype/Data/Skills/" + skillType.ToString() + "/" + fileName + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Skill", "A skill with name " + fileName + " already exists. Replace with this skill?", "Yes", "No")) {
                    File.Delete(path);
                    SaveSkillJson(path);
                }
            } else {
                SaveSkillJson(path);
            }
        }
        private void SaveSkillJson(string path) {
            if(skillType == SKILL_TYPE.ATTACK) {
                SaveAttackSkill(path);
            } else if (skillType == SKILL_TYPE.HEAL) {
                SaveHealSkill(path);
            } else if (skillType == SKILL_TYPE.OBTAIN_ITEM) {
                SaveObtainSkill(path);
            } else if (skillType == SKILL_TYPE.MOVE) {
                SaveMoveSkill(path);
            } else {
                SaveFleeSkill(path);
            }

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved skill at " + path);
        }
        private void SetCommonData(Skill newSkill) {
            newSkill.skillName = this.skillName;
            newSkill.activationWeight = this.activationWeight;
            newSkill.accuracy = this.accuracy;
            newSkill.range = this.range;
            newSkill.skillRequirements = this.skillRequirements;
            newSkill.attributeModifier = this.attributeModifier;
        }
        private void SaveAttackSkill(string path) {
            AttackSkill newSkill = new AttackSkill();

            SetCommonData(newSkill);

            newSkill.attackPower = attackPower;
            newSkill.attackType = attackType;
            newSkill.statusEffect = statusEffect;
            newSkill.statusEffectRate = statusEffectRate;
            newSkill.injuryRate = injuryRate;
            newSkill.decapitationRate = decapitationRate;

            SaveJson(newSkill, path);
        }
        private void SaveHealSkill(string path) {
            HealSkill newSkill = new HealSkill();
            SetCommonData(newSkill);
            newSkill.healPower = healPower;
            SaveJson(newSkill, path);
        }
        private void SaveFleeSkill(string path) {
            FleeSkill newSkill = new FleeSkill();
            SetCommonData(newSkill);
            SaveJson(newSkill, path);
        }
        private void SaveObtainSkill(string path) {
            ObtainSkill newSkill = new ObtainSkill();
            SetCommonData(newSkill);
            SaveJson(newSkill, path);
        }
        private void SaveMoveSkill(string path) {
            MoveSkill newSkill = new MoveSkill();
            SetCommonData(newSkill);
            SaveJson(newSkill, path);
        }
        private void SaveJson(Skill skill, string path) {
            string jsonString = JsonUtility.ToJson(skill);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
        }
        #endregion

        #region Loading
        private void LoadSkill() {
            string filePath = EditorUtility.OpenFilePanel("Select Skill Json", "Assets/CombatPrototype/Data/Skills/", "json");
            if (!string.IsNullOrEmpty(filePath)) {
                string dataAsJson = File.ReadAllText(filePath);
                if (filePath.Contains("ATTACK")) {
                    AttackSkill currSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
                    LoadAttackSkill(currSkill);
                } else if (filePath.Contains("HEAL")) {
                    HealSkill currSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
                    LoadHealSkill(currSkill);
                } else if (filePath.Contains("OBTAIN")) {
                    ObtainSkill currSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
                    LoadObtainSkill(currSkill);
                } else if (filePath.Contains("FLEE")) {
                    FleeSkill currSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
                    LoadFleeSkill(currSkill);
                } else if (filePath.Contains("MOVE")) {
                    MoveSkill currSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
                    LoadMoveSkill(currSkill);
                }
            }
        }
        private void LoadCommonData(Skill skill) {
            skillName = skill.skillName;
            activationWeight = skill.activationWeight;
            accuracy = skill.accuracy;
            skillRequirements = skill.skillRequirements;
        }
        private void LoadAttackSkill(AttackSkill skill) {
            skillType = SKILL_TYPE.ATTACK;
            LoadCommonData(skill);

            //Attack Skill Fields
            attackPower = skill.attackPower;
            attackType = skill.attackType;
            statusEffect = skill.statusEffect;
            statusEffectRate = skill.statusEffectRate;
            injuryRate = skill.injuryRate;
            decapitationRate = skill.decapitationRate;
        }
        private void LoadHealSkill(HealSkill skill) {
            skillType = SKILL_TYPE.HEAL;
            LoadCommonData(skill);

            //Heal Skill Fields
            healPower = skill.healPower;
        }
        private void LoadObtainSkill(ObtainSkill skill) {
            skillType = SKILL_TYPE.OBTAIN_ITEM;
            LoadCommonData(skill);
        }
        private void LoadFleeSkill(FleeSkill skill) {
            skillType = SKILL_TYPE.FLEE;
            LoadCommonData(skill);
        }
        private void LoadMoveSkill(MoveSkill skill) {
            skillType = SKILL_TYPE.MOVE;
            LoadCommonData(skill);
        }
        #endregion
    }
}

