using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class SkillRequirement {
        public int itemQuantity;
        //public SKILL_REQUIREMENT_ITEM requirementItem;
		public IBodyPart.ATTRIBUTE attributeRequired;
    }
}