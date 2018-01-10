using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class AttackSkill : Skill {
        public ATTACK_TYPE attackType;
        public int durabilityDamage = 1;
        public int durabilityCost = 1;
		public List<StatusEffectRate> statusEffectRates;
    }
}