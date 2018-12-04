using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : Item {
	[SerializeField] private WEAPON_TYPE _weaponType;
    //public MATERIAL material;
    //public QUALITY quality;
    [SerializeField] private float _weaponPower;
    [SerializeField] private WEAPON_PREFIX _prefix;
    [SerializeField] private WEAPON_SUFFIX _suffix;
    [SerializeField] private ELEMENT _element;
	//public float damageRange;
	//public List<IBodyPart.ATTRIBUTE> attributes;
    //public List<IBodyPart.ATTRIBUTE> equipRequirements;

    //public List<string> attackSkills = new List<string>();
    //public List<string> healSkills = new List<string>();

    //private List<Skill> _skills;

    #region getters/setters
    public WEAPON_TYPE weaponType {
        get { return _weaponType; }
        set { _weaponType = value; }
    }
    public ELEMENT element {
        get { return _element; }
        set { _element = value; }
    }
    public WEAPON_PREFIX prefixType {
        get { return _prefix; }
    }
    public WEAPON_SUFFIX suffixType {
        get { return _suffix; }
    }
    public float weaponPower {
        get { return _weaponPower; }
        set { _weaponPower = value; }
    }
    public float attackPower {
        get { return (_weaponPower + (prefix.flatModifier + suffix.flatModifier)) * (1f + ((prefix.percentModifier + suffix.percentModifier) / 100f)); }
    }
    public WeaponPrefix prefix {
        get {
            if(ItemManager.Instance != null){
                return ItemManager.Instance.weaponPrefixes[_prefix];
            }
            return CombatSimManager.Instance.weaponPrefixes[_prefix];
        }
    }
    public WeaponSuffix suffix {
        get {
            if (ItemManager.Instance != null) {
                return ItemManager.Instance.weaponSuffixes[_suffix];
            }
            return CombatSimManager.Instance.weaponSuffixes[_suffix];
        }
    }
    #endregion

    public void SetPrefix(WEAPON_PREFIX prefix) {
        _prefix = prefix;
    }
    public void SetSuffix(WEAPON_SUFFIX suffix) {
        _suffix = suffix;
    }
    //		public Weapon(){
    //			ConstructAllSkillsList ();
    //		}

    //		public void AddSkill(Skill skillToAdd) {
    //			if(skillToAdd is AttackSkill){
    //				attackSkills.Add(skillToAdd.skillName);
    //			}else if(skillToAdd is HealSkill){
    //				healSkills.Add(skillToAdd.skillName);
    //			}
    ////			switch (skillToAdd) {
    ////			case SKILL_TYPE.ATTACK:
    ////				attackSkills.Add(skillToAdd.skillName);
    ////				break;
    ////			}
    //		}

    /*
        This is for the main game that uses the SkillManager to create skills.
            */
    //public void ConstructSkillsList() {
    //    _skills = new List<Skill>();
    //    if (SkillManager.Instance.weaponTypeSkills.ContainsKey(this.weaponType)) {
    //        List<Skill> weaponTypeSkills = SkillManager.Instance.weaponTypeSkills[weaponType];
    //        for (int i = 0; i < weaponTypeSkills.Count; i++) {
    //            Skill newSkill = weaponTypeSkills[i].CreateNewCopy();
    //            //newSkill.weapon = this;
    //            _skills.Add(newSkill);
    //        }
    //    }

    //    for (int i = 0; i < attackSkills.Count; i++) {
    //        string skillName = attackSkills[i];
    //        AttackSkill currSkill = (AttackSkill)SkillManager.Instance.CreateNewSkillInstance(skillName);
    //        //currSkill.weapon = this;
    //        _skills.Add(currSkill);
    //    }
    //    for (int i = 0; i < healSkills.Count; i++) {
    //        string skillName = healSkills[i];
    //        HealSkill currSkill = (HealSkill)SkillManager.Instance.CreateNewSkillInstance(skillName);
    //        //currSkill.weapon = this;
    //        _skills.Add(currSkill);
    //    }
    //}

    #region overrides
    public override Item CreateNewCopy() {
        if (isStackable) {
            return ItemManager.Instance.allWeapons[itemName];
        }
        Weapon copy = new Weapon();
        copy.weaponType = weaponType;
        copy.weaponPower = weaponPower;
        copy._prefix = _prefix;
        copy._suffix = _suffix;
        copy.element = element;
        SetCommonData(copy);
        return copy;
    }
    #endregion
}