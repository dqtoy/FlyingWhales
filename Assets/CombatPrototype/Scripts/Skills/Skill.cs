using UnityEngine;
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