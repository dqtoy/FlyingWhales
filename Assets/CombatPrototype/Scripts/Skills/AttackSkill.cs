using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
//    [System.Serializable]
    public class AttackSkill : Skill {
        public ATTACK_TYPE attackType;
        public int durabilityDamage;
        public int durabilityCost;
		public List<StatusEffectRate> statusEffectRates;
    }
}