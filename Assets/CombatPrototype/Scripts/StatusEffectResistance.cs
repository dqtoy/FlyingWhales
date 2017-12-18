using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public struct StatusEffectResistance {
        public STATUS_EFFECT statusEffect;
		public float percentage;
    }
}