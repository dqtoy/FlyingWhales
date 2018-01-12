using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class SkillComponent : MonoBehaviour {
		public SKILL_TYPE skillType;
		public string skillName;
		public string description;
		public ACTIVATION_WEIGHT_TYPE actWeightType;
		public int activationWeight;
		public float accuracy;
		public int range;
        public SkillRequirement[] skillRequirements;

		//Attack Skill Fields
		public ATTACK_TYPE attackType;
		public SKILL_CATEGORY skillCategory;
        public int durabilityDamage;
		public List<StatusEffectRate> statusEffectRates;

		//Heal Skill Fields
		public int healPower;

        //Shared Fields
        public int durabilityCost;
    }
}

