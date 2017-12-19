using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace ECS {
	[CustomEditor(typeof(SkillComponent))]
    public class SkillCreator : Editor {
		SkillComponent skillComponent;
        public override void OnInspectorGUI() {
			if(skillComponent == null){
				skillComponent = (SkillComponent)target;
			}

            GUILayout.Label("Skill Creator ", EditorStyles.boldLabel);
			skillComponent.skillType = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type: ", skillComponent.skillType);
			skillComponent.skillName = EditorGUILayout.TextField("Skill Name: ", skillComponent.skillName);
			skillComponent.description = EditorGUILayout.TextField("Description: ", skillComponent.description);
			skillComponent.activationWeight = EditorGUILayout.IntField("Activation Weight: ", skillComponent.activationWeight);
			skillComponent.range = EditorGUILayout.IntField("Range: ", skillComponent.range);
			skillComponent.accuracy = EditorGUILayout.Slider("Accuracy: ", skillComponent.accuracy, 0f, 100f);
			skillComponent.attributeModifier = (CHARACTER_ATTRIBUTES)EditorGUILayout.EnumPopup("Attribute Modifier: ", skillComponent.attributeModifier);
            skillComponent.strengthPower = EditorGUILayout.FloatField("Strength Power: ", skillComponent.strengthPower);
            skillComponent.intellectPower = EditorGUILayout.FloatField("Intellect Power: ", skillComponent.intellectPower);
            skillComponent.agilityPower = EditorGUILayout.FloatField("Agility Power: ", skillComponent.agilityPower);
			skillComponent.levelRequirement = EditorGUILayout.IntField("Level Requirement: ", skillComponent.levelRequirement);

            switch (skillComponent.skillType) {
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

            SerializedProperty serializedProperty = serializedObject.FindProperty("skillRequirements");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save Skill")) {
				SaveSkill(skillComponent.skillName);
            }
        }

        private void ShowAttackSkillFields() {
			skillComponent.attackType = (ATTACK_TYPE)EditorGUILayout.EnumPopup("Attack Type: ", skillComponent.attackType);
			skillComponent.statusEffect = (STATUS_EFFECT)EditorGUILayout.EnumPopup("Status Effect: ", skillComponent.statusEffect);
			if(skillComponent.statusEffect != STATUS_EFFECT.NONE) {
                EditorGUI.indentLevel++;
				skillComponent.statusEffectRate = EditorGUILayout.IntField("Status Effect Rate: ", skillComponent.statusEffectRate);
                EditorGUI.indentLevel--;
            }
			skillComponent.injuryRate = EditorGUILayout.IntField("Injury Rate: ", skillComponent.injuryRate);
			skillComponent.decapitationRate = EditorGUILayout.IntField("Decapitation Rate: ", skillComponent.decapitationRate);
            skillComponent.durabilityDamage = EditorGUILayout.IntField("Durability Damage: ", skillComponent.durabilityDamage);
            skillComponent.durabilityCost = EditorGUILayout.IntField("Durability Cost: ", skillComponent.durabilityCost);
        }
        private void ShowHealSkillFields() {
			skillComponent.healPower = EditorGUILayout.IntField("Heal Power: ",skillComponent. healPower);
            skillComponent.durabilityCost = EditorGUILayout.IntField("Durability Cost: ", skillComponent.durabilityCost);
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
			string path = "Assets/CombatPrototype/Data/Skills/" + skillComponent.skillType.ToString() + "/" + fileName + ".json";
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
			if(skillComponent.skillType == SKILL_TYPE.ATTACK) {
                SaveAttackSkill(path);
			} else if (skillComponent.skillType == SKILL_TYPE.HEAL) {
                SaveHealSkill(path);
			} else if (skillComponent.skillType == SKILL_TYPE.OBTAIN_ITEM) {
                SaveObtainSkill(path);
			} else if (skillComponent.skillType == SKILL_TYPE.MOVE) {
                SaveMoveSkill(path);
            } else {
                SaveFleeSkill(path);
            }

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved skill at " + path);
        }
        private void SetCommonData(Skill newSkill) {
			newSkill.skillName = skillComponent.skillName;
			newSkill.description = skillComponent.description;
			newSkill.activationWeight = skillComponent.activationWeight;
			newSkill.accuracy = skillComponent.accuracy;
			newSkill.range = skillComponent.range;
			newSkill.skillRequirements = skillComponent.skillRequirements;
			newSkill.attributeModifier = skillComponent.attributeModifier;
            newSkill.strengthPower = skillComponent.strengthPower;
            newSkill.intellectPower = skillComponent.intellectPower;
            newSkill.agilityPower = skillComponent.agilityPower;
			newSkill.levelRequirement = skillComponent.levelRequirement;
        }
        private void SaveAttackSkill(string path) {
            AttackSkill newSkill = new AttackSkill();

            SetCommonData(newSkill);

			newSkill.attackType = skillComponent.attackType;
			newSkill.statusEffect = skillComponent.statusEffect;
			newSkill.statusEffectRate = skillComponent.statusEffectRate;
			newSkill.injuryRate = skillComponent.injuryRate;
			newSkill.decapitationRate = skillComponent.decapitationRate;
            newSkill.durabilityDamage = skillComponent.durabilityDamage;
            newSkill.durabilityCost = skillComponent.durabilityCost;

            SaveJson(newSkill, path);
        }
        private void SaveHealSkill(string path) {
            HealSkill newSkill = new HealSkill();
            SetCommonData(newSkill);
			newSkill.healPower = skillComponent.healPower;
            newSkill.durabilityCost = skillComponent.durabilityCost;
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
//        private void LoadSkill() {
//            string filePath = EditorUtility.OpenFilePanel("Select Skill Json", "Assets/CombatPrototype/Data/Skills/", "json");
//            if (!string.IsNullOrEmpty(filePath)) {
//                string dataAsJson = File.ReadAllText(filePath);
//                if (filePath.Contains("ATTACK")) {
//                    AttackSkill currSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
//                    LoadAttackSkill(currSkill);
//                } else if (filePath.Contains("HEAL")) {
//                    HealSkill currSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
//                    LoadHealSkill(currSkill);
//                } else if (filePath.Contains("OBTAIN")) {
//                    ObtainSkill currSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
//                    LoadObtainSkill(currSkill);
//                } else if (filePath.Contains("FLEE")) {
//                    FleeSkill currSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
//                    LoadFleeSkill(currSkill);
//                } else if (filePath.Contains("MOVE")) {
//                    MoveSkill currSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
//                    LoadMoveSkill(currSkill);
//                }
//            }
//        }
//        private void LoadCommonData(Skill skill) {
//            skillName = skill.skillName;
//            activationWeight = skill.activationWeight;
//            accuracy = skill.accuracy;
//            range = skill.range;
//            skillRequirements = skill.skillRequirements;
//        }
//        private void LoadAttackSkill(AttackSkill skill) {
//            skillType = SKILL_TYPE.ATTACK;
//            LoadCommonData(skill);
//
//            //Attack Skill Fields
//            attackPower = skill.attackPower;
//            attackType = skill.attackType;
//            statusEffect = skill.statusEffect;
//            statusEffectRate = skill.statusEffectRate;
//            injuryRate = skill.injuryRate;
//            decapitationRate = skill.decapitationRate;
//        }
//        private void LoadHealSkill(HealSkill skill) {
//            skillType = SKILL_TYPE.HEAL;
//            LoadCommonData(skill);
//
//            //Heal Skill Fields
//            healPower = skill.healPower;
//        }
//        private void LoadObtainSkill(ObtainSkill skill) {
//            skillType = SKILL_TYPE.OBTAIN_ITEM;
//            LoadCommonData(skill);
//        }
//        private void LoadFleeSkill(FleeSkill skill) {
//            skillType = SKILL_TYPE.FLEE;
//            LoadCommonData(skill);
//        }
//        private void LoadMoveSkill(MoveSkill skill) {
//            skillType = SKILL_TYPE.MOVE;
//            LoadCommonData(skill);
//        }
        #endregion
    }
}

