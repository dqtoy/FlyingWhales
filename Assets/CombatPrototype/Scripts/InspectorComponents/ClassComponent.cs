using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ClassComponent : MonoBehaviour {
		public string className;
		public List<string> attackSkills = new List<string>();
		public List<string> healSkills = new List<string>();
		public List<string> fleeSkills = new List<string>();
		public List<string> moveSkills = new List<string>();
		public List<string> obtainSkills = new List<string>();
		public int actRate;
		public int strGain;
		public int intGain;
		public int agiGain;
		public int hpGain;
		public int dodgeRate;
		public int parryRate;
		public int blockRate;

		[SerializeField] private List<Skill> _skills;

		internal bool skillsFoldout;
		internal SKILL_TYPE skillTypeToAdd;
		internal int skillToAddIndex;

		#region getters/setters
		public List<Skill> skills {
			get { return _skills; }
		}
		#endregion

		public void AddSkillOfType(SKILL_TYPE skillType, Skill skillToAdd) {
			switch (skillType) {
			case SKILL_TYPE.ATTACK:
				attackSkills.Add (skillToAdd.skillName);
				break;
			case SKILL_TYPE.HEAL:
				healSkills.Add(skillToAdd.skillName);
				break;
			case SKILL_TYPE.OBTAIN_ITEM:
				obtainSkills.Add(skillToAdd.skillName);
				break;
			case SKILL_TYPE.FLEE:
				fleeSkills.Add(skillToAdd.skillName);
				break;
			case SKILL_TYPE.MOVE:
				moveSkills.Add(skillToAdd.skillName);
				break;
			}
			if(this._skills == null){
				this._skills = new List<Skill> ();
			}
			this._skills.Add (skillToAdd);
		}
	}

}
