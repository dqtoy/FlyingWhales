using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Skill {
    public string skillName;
    public string description;
    public SKILL_TYPE skillType;
	//public SKILL_CATEGORY skillCategory;
    //public ACTIVATION_WEIGHT_TYPE actWeightType;
    //public int activationWeight;
    //public float accuracy;
    //public int range;
    public TARGET_TYPE targetType;
    public ELEMENT element;
    //public int numOfRowsHit;
    //public WEAPON_TYPE[] allowedWeaponTypes;

    //Not part of skill creator
    public bool isEnabled;

    public Skill CreateNewCopy(){
        Skill newSkill = new Skill();
        SetCommonData(newSkill);
		//if (this is AttackSkill) {
		//	AttackSkill attackSkill = this as AttackSkill;
		//	AttackSkill newAttackSkill = new AttackSkill ();
//             newAttackSkill.power = attackSkill.power;
//             newAttackSkill.spCost = attackSkill.spCost;
//             newAttackSkill.element = attackSkill.element;
		//	SetCommonData (newAttackSkill);
		//	return newAttackSkill;
		//} else if(this is HealSkill){
		//	HealSkill healSkill = this as HealSkill;
		//	HealSkill newHealSkill = new HealSkill ();
		//	newHealSkill.healPower = healSkill.healPower;
		//	SetCommonData (newHealSkill);
		//	return newHealSkill;
//         } else if (this is MoveSkill) {
//             MoveSkill newSkill = new MoveSkill();
//             SetCommonData(newSkill);
//             return newSkill;
//         } else if (this is FleeSkill) {
//             FleeSkill newSkill = new FleeSkill();
//             SetCommonData(newSkill);
//             return newSkill;
//         }
        return newSkill;
	}
	public void SetCommonData(Skill skill){
		skill.skillType = this.skillType;
		skill.skillName = this.skillName;
		skill.description = this.description;
		//skill.skillCategory = this.skillCategory;
		//skill.actWeightType = this.actWeightType;
		//skill.activationWeight = this.activationWeight;
		//skill.accuracy = this.accuracy;
		//skill.range = this.range;
        skill.targetType = this.targetType;
        skill.element = this.element;
        //skill.numOfRowsHit = this.numOfRowsHit;

        skill.isEnabled = this.isEnabled;
        //if(this.allowedWeaponTypes != null && this.allowedWeaponTypes.Length > 0) {
        //    skill.allowedWeaponTypes = new WEAPON_TYPE[this.allowedWeaponTypes.Length];
        //    Array.Copy(this.allowedWeaponTypes, skill.allowedWeaponTypes, this.allowedWeaponTypes.Length);
        //}
        //if(this.skillRequirements != null && this.skillRequirements.Length > 0) {
        //    skill.skillRequirements = new SkillRequirement[this.skillRequirements.Length];
        //    for (int i = 0; i < this.skillRequirements.Length; i++) {
        //        skill.skillRequirements[i] = new SkillRequirement();
        //        skill.skillRequirements[i].attributeRequired = this.skillRequirements[i].attributeRequired;
        //        skill.skillRequirements[i].itemQuantity = this.skillRequirements[i].itemQuantity;
        //    }
        //}
	}
}