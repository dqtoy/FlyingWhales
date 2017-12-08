using UnityEngine;
using System.Collections;

namespace ECS {
    public class AttackSkill : Skill {
        public int _attackPower;
        public ATTACK_TYPE _attackType;
        public STATUS_EFFECT _statusEffect;
        public int statusEffectRate;
    }
}