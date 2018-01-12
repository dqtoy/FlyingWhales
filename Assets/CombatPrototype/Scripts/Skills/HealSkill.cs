using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class HealSkill : Skill {
        public int healPower;
        public int durabilityCost;
    }
}