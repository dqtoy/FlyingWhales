using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
    public TextAsset skill;
    //public int dodgeRate;
    //public int parryRate;
    //public int blockRate;

    public List<RESOURCE> harvestResources;
    public List<TextAsset> weaponTiers;
    public List<TextAsset> armorTiers;
    public List<TextAsset> accessoryTiers;

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