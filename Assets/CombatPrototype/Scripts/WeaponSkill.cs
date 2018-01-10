using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	public class WeaponSkill {
		public WEAPON_TYPE weaponType;
		public List<IBodyPart.ATTRIBUTE> equipRequirements;
		public List<Skill> skills;
		public List<string> attackSkills = new List<string>();
		public List<string> healSkills = new List<string>();

		public void AddSkill(Skill skillToAdd) {
			if(skillToAdd is AttackSkill){
				attackSkills.Add(skillToAdd.skillName);
			}else if(skillToAdd is HealSkill){
				healSkills.Add(skillToAdd.skillName);
			}
		}

		public void ConstructWeaponSkillsList() {
			skills = new List<Skill>();
			for (int i = 0; i < attackSkills.Count; i++) {
				string skillName = attackSkills[i];
				string path = "Assets/CombatPrototype/Data/Skills/WEAPON/ATTACK/" + skillName + ".json";
				AttackSkill currSkill = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText(path));
				skills.Add(currSkill);
			}
			for (int i = 0; i < healSkills.Count; i++) {
				string skillName = healSkills[i];
				string path = "Assets/CombatPrototype/Data/Skills/WEAPON/HEAL/" + skillName + ".json";
				HealSkill currSkill = JsonUtility.FromJson<HealSkill>(System.IO.File.ReadAllText(path));
				skills.Add(currSkill);
			}
		}
	}
}
