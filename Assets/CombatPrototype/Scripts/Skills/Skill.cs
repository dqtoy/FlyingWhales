using UnityEngine;
using System.Collections;
using System;

namespace ECS {
    [System.Serializable]
    public class Skill {
        public string skillName;
        public int activationWeight;
        public float accuracy;
        public SkillRequirement[] skillRequirements;
    }
}