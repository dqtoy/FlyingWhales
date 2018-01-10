using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Weapon : Item {
		public WEAPON_TYPE weaponType;
		public MATERIAL material;
		public QUALITY quality;
		public float weaponPower;
		public List<IBodyPart.ATTRIBUTE> attributes;
        public List<IBodyPart.ATTRIBUTE> equipRequirements;
		internal List<IBodyPart> bodyPartsAttached = new List<IBodyPart>();

		public List<string> attackSkills = new List<string>();
		public List<string> healSkills = new List<string>();

		private List<Skill> _skills;

		#region getters/setters
		public List<Skill> skills {
			get { return _skills; }
		}
		#endregion

//		public Weapon(){
//			ConstructAllSkillsList ();
//		}

		public void AddSkill(Skill skillToAdd) {
			if(skillToAdd is AttackSkill){
				attackSkills.Add(skillToAdd.skillName);
			}else if(skillToAdd is HealSkill){
				healSkills.Add(skillToAdd.skillName);
			}
//			switch (skillToAdd) {
//			case SKILL_TYPE.ATTACK:
//				attackSkills.Add(skillToAdd.skillName);
//				break;
//			}
		}

		public void ConstructAllSkillsList() {
			_skills = new List<Skill>();
			if(CombatPrototypeManager.Instance.weaponTypeSkills.ContainsKey(this.weaponType)){
				List<Skill> weaponTypeSkills = CombatPrototypeManager.Instance.weaponTypeSkills [weaponType];
				for (int i = 0; i < weaponTypeSkills.Count; i++) {
					_skills.Add (weaponTypeSkills [i].CreateNewCopy ());
				}
			}

			for (int i = 0; i < attackSkills.Count; i++) {
				string skillName = attackSkills[i];
				string path = "Assets/CombatPrototype/Data/Skills/WEAPON/ATTACK/" + skillName + ".json";
				AttackSkill currSkill = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText(path));
				currSkill.weapon = this;
				_skills.Add(currSkill);
			}
			for (int i = 0; i < healSkills.Count; i++) {
				string skillName = healSkills[i];
				string path = "Assets/CombatPrototype/Data/Skills/WEAPON/HEAL/" + skillName + ".json";
				HealSkill currSkill = JsonUtility.FromJson<HealSkill>(System.IO.File.ReadAllText(path));
				currSkill.weapon = this;
				_skills.Add(currSkill);
			}


		}
	}
}
