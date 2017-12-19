using UnityEngine;
using System.Collections;
using System;

namespace ECS {
    [System.Serializable]
    public class Skill {
        public string skillName;
		public string description;
        public int activationWeight;
        public float accuracy;
        public int range;
		public bool isEnabled;
        public float strengthPower;
        public float intellectPower;
        public float agilityPower;
        public SkillRequirement[] skillRequirements;
        public CHARACTER_ATTRIBUTES attributeModifier;
		public int levelRequirement;

        public bool RequiresItem() {
            for (int i = 0; i < skillRequirements.Length; i++) {
                SkillRequirement skillReq = skillRequirements[i];
                if(skillReq.equipmentType != EQUIPMENT_TYPE.NONE) {
                    return true;
                }
            }
            return false;
        }
    }
}