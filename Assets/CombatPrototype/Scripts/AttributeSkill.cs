using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	public class AttributeSkill {
		public SkillRequirement[] requirements;
		public List<Skill> skills;
		public List<string> attackSkills = new List<string>();
		public List<string> healSkills = new List<string>();

		public void AddSkill(Skill skillToAdd) {
			if(skillToAdd.skillType == SKILL_TYPE.ATTACK){
				attackSkills.Add(skillToAdd.skillName);
			}else if(skillToAdd.skillType == SKILL_TYPE.HEAL){
				healSkills.Add(skillToAdd.skillName);
			}
		}

        /*
         This is for the main game that uses the SkillManager to create skills.
             */
        public void ConstructSkillList() {
            for (int i = 0; i < attackSkills.Count; i++) {
                string skillName = attackSkills[i];
                skills.Add(SkillManager.Instance.CreateNewSkillInstance(skillName));
            }
            for (int i = 0; i < healSkills.Count; i++) {
                string skillName = healSkills[i];
                skills.Add(SkillManager.Instance.CreateNewSkillInstance(skillName));
            }
        }

		public void ConstructAttributeSkillsList() {
			skills = new List<Skill>();
			for (int i = 0; i < attackSkills.Count; i++) {
				string skillName = attackSkills[i];
				string path = "Assets/CombatPrototype/Data/Skills/BODY_PART/ATTACK/" + skillName + ".json";
				AttackSkill currSkill = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText(path));
				skills.Add(currSkill);
			}
			for (int i = 0; i < healSkills.Count; i++) {
				string skillName = healSkills[i];
				string path = "Assets/CombatPrototype/Data/Skills/BODY_PART/HEAL/" + skillName + ".json";
				HealSkill currSkill = JsonUtility.FromJson<HealSkill>(System.IO.File.ReadAllText(path));
				skills.Add(currSkill);
			}
		}
	}
}
