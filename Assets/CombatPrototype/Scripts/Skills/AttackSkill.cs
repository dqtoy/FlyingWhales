using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class AttackSkill : Skill {
        public int attackPower;
        public ATTACK_TYPE attackType;
        public STATUS_EFFECT statusEffect;
        public int statusEffectRate;
        public int injuryRate;
        public int decapitationRate;
        public int durabilityDamage;
        public int durabilityCost;
    }
}