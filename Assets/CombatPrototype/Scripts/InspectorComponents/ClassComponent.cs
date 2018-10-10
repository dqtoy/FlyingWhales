using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ClassComponent : MonoBehaviour {
		public string className;
		public float baseAttackPower;
		public float attackPowerPerLevel;
		public float baseSpeed;
		public float speedPerLevel;
        public int baseHP;
        public int hpPerLevel;
        public int baseSP;
        public int spPerLevel;
        public ACTION_TYPE workActionType;
        //public int dodgeRate;
        //public int parryRate;
        //public int blockRate;

        public List<WEAPON_TYPE> allowedWeaponTypes;
        public List<RESOURCE> harvestResources;
        public List<TextAssetListWrapper> skillsPerLevel;

        //public List<StringListWrapper> skillsPerLevelNames;
//		public void AddSkillOfType(SKILL_TYPE skillType, Skill skillToAdd) {
//			switch (skillType) {
//			case SKILL_TYPE.ATTACK:
//				attackSkills.Add (skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.HEAL:
//				healSkills.Add(skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.OBTAIN_ITEM:
//				obtainSkills.Add(skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.FLEE:
//				fleeSkills.Add(skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.MOVE:
//				moveSkills.Add(skillToAdd.skillName);
//				break;
//			}
//			if(this._skills == null){
//				this._skills = new List<Skill> ();
//			}
//			this._skills.Add (skillToAdd);
//		}
    }

}
