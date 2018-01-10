using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class AttributeSkillComponent : MonoBehaviour {
		public string fileName;
		public SkillRequirement[] requirements;
		public List<Skill> skills;

		internal bool skillsFoldout;
		internal SKILL_TYPE skillTypeToAdd;
		internal int skillToAddIndex;

		public void AddSkill(Skill skillToAdd) {
			if(this.skills == null){
				this.skills = new List<Skill> ();	
			}
			this.skills.Add (skillToAdd);
		}
    }
}

