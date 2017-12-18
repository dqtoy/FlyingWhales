using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class WeaponRequirement {
        public IBodyPart.ATTRIBUTE attribute;
        public int quantity;
    }
}