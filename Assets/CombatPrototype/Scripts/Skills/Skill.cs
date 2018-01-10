﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ECS {
    [System.Serializable]
    public class Skill {
        public string skillName;
		public string description;
		public SKILL_CATEGORY skillCategory;
		public ACTIVATION_WEIGHT_TYPE actWeightType;
        public int activationWeight;
        public float accuracy;
        public int range;
		public bool isEnabled;
        public SkillRequirement[] skillRequirements;

		internal Weapon weapon;

		public Skill CreateNewCopy(){
			if (this is AttackSkill) {
				AttackSkill attackSkill = (AttackSkill)this;
				AttackSkill newAttackSkill = new AttackSkill ();
				newAttackSkill.attackType = attackSkill.attackType;
				newAttackSkill.durabilityDamage = attackSkill.durabilityDamage;
				newAttackSkill.durabilityCost = attackSkill.durabilityCost;
				newAttackSkill.statusEffectRates = attackSkill.statusEffectRates;
				SetCommonData (newAttackSkill);
				return newAttackSkill;
			} else if(this is HealSkill){
				HealSkill healSkill = (HealSkill)this;
				HealSkill newHealSkill = new HealSkill ();
				newHealSkill.healPower = healSkill.healPower;
				newHealSkill.durabilityCost = healSkill.durabilityCost;
				SetCommonData (newHealSkill);
				return newHealSkill;
			}
			return null;
		}
		public void SetCommonData(Skill skill){
			skill.skillName = this.skillName;
			skill.description = this.description;
			skill.skillCategory = this.skillCategory;
			skill.actWeightType = this.actWeightType;
			skill.activationWeight = this.activationWeight;
			skill.accuracy = this.accuracy;
			skill.range = this.range;
			skill.isEnabled = this.isEnabled;
			skill.skillRequirements = new SkillRequirement[this.skillRequirements.Length];
			for (int i = 0; i < this.skillRequirements.Length; i++) {
				skill.skillRequirements [i] = new SkillRequirement ();
				skill.skillRequirements [i].attributeRequired = this.skillRequirements [i].attributeRequired;
				skill.skillRequirements [i].itemQuantity = this.skillRequirements [i].itemQuantity;
			}

		}
//        public bool RequiresItem() {
//            for (int i = 0; i < skillRequirements.Length; i++) {
//                SkillRequirement skillReq = skillRequirements[i];
//                if(skillReq.equipmentType != EQUIPMENT_TYPE.NONE) {
//                    return true;
//                }
//            }
//            return false;
//        }
    }
}