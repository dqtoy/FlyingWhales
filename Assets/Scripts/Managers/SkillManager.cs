using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class SkillManager : MonoBehaviour {

    public static SkillManager Instance = null;

    public Dictionary<string, ECS.Skill> allSkills;
    public ECS.AttributeSkill[] attributeSkills;
    public Dictionary<WEAPON_TYPE, List<ECS.Skill>> weaponTypeSkills = new Dictionary<WEAPON_TYPE, List<ECS.Skill>>();

    private void Awake() {
        Instance = this;

    }
	internal void Initialize(){
		ConstructAllSkillsList();
		ConstructAttributeSkills();
		ConstructWeaponTypeSkills();
	}
    private void ConstructAllSkillsList() {
        allSkills = new Dictionary<string, ECS.Skill>();
        string path = "Assets/CombatPrototype/Data/Skills/";
        string[] directories = Directory.GetDirectories(path); //Get first level skill types
        for (int i = 0; i < directories.Length; i++) {
            string currDirectory = directories[i];
            string[] subDirectories = Directory.GetDirectories(currDirectory);
            for (int j = 0; j < subDirectories.Length; j++) {
                string currSubDirectory = subDirectories[j];
                string skillType = new DirectoryInfo(currSubDirectory).Name;
                string[] files = Directory.GetFiles(currSubDirectory, "*.json");
                for (int k = 0; k < files.Length; k++) {
                    string currFilePath = files[k];
                    string dataAsJson = File.ReadAllText(currFilePath);
                    SKILL_TYPE currSkillType = (SKILL_TYPE)Enum.Parse(typeof(SKILL_TYPE), skillType);
                    switch (currSkillType) {
                        case SKILL_TYPE.ATTACK:
                            ECS.AttackSkill attackSkill = JsonUtility.FromJson<ECS.AttackSkill>(dataAsJson);
                            allSkills.Add(attackSkill.skillName, attackSkill);
                            break;
                        case SKILL_TYPE.HEAL:
                            ECS.HealSkill healSkill = JsonUtility.FromJson<ECS.HealSkill>(dataAsJson);
                            allSkills.Add(healSkill.skillName, healSkill);
                            break;
                        case SKILL_TYPE.OBTAIN_ITEM:
                            ECS.ObtainSkill obtainSkill = JsonUtility.FromJson<ECS.ObtainSkill>(dataAsJson);
                            allSkills.Add(obtainSkill.skillName, obtainSkill);
                            break;
                        case SKILL_TYPE.FLEE:
                            ECS.FleeSkill fleeSkill = JsonUtility.FromJson<ECS.FleeSkill>(dataAsJson);
                            allSkills.Add(fleeSkill.skillName, fleeSkill);
                            break;
                        case SKILL_TYPE.MOVE:
                            ECS.MoveSkill moveSkill = JsonUtility.FromJson<ECS.MoveSkill>(dataAsJson);
                            allSkills.Add(moveSkill.skillName, moveSkill);
                            break;
                        default:
                            break;
                    }

                }
            }
        }
    }

    public ECS.Skill CreateNewSkillInstance(string skillName) {
        if (allSkills.ContainsKey(skillName)) {
            return allSkills[skillName].CreateNewCopy();
        }
        throw new System.Exception("There is no skill called " + skillName);
    }

    private void ConstructAttributeSkills() {
        string path = "Assets/CombatPrototype/Data/AttributeSkills/";
        string[] attributeSkillsJson = System.IO.Directory.GetFiles(path, "*.json");
        attributeSkills = new ECS.AttributeSkill[attributeSkillsJson.Length];
        for (int i = 0; i < attributeSkillsJson.Length; i++) {
            string file = attributeSkillsJson[i];
            string dataAsJson = System.IO.File.ReadAllText(file);
            ECS.AttributeSkill attSkill = JsonUtility.FromJson<ECS.AttributeSkill>(dataAsJson);
            attSkill.ConstructSkillList();
            attributeSkills[i] = attSkill;
        }
    }

    private void ConstructWeaponTypeSkills() {
        string path = "Assets/CombatPrototype/Data/WeaponTypes/";
        string[] weaponTypesJson = System.IO.Directory.GetFiles(path, "*.json");
		for (int i = 0; i < weaponTypesJson.Length; i++) {
			string file = weaponTypesJson[i];
            string dataAsJson = System.IO.File.ReadAllText(file);
			ECS.WeaponType weapType = JsonUtility.FromJson<ECS.WeaponType>(dataAsJson);
			weapType.ConstructWeaponSkillsList();
			weaponTypeSkills.Add(weapType.weaponType, new List<ECS.Skill>(weapType.skills));
        }
    }
}
