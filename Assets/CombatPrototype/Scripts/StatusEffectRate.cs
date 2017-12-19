using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public struct StatusEffectRate {
        public STATUS_EFFECT statusEffect;
		public int ratePercentage;
    }
}