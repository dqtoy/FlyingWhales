using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class SkillRequirement {
        public SKILL_REQUIREMENT_TYPE requirementType;
        public SKILL_REQUIREMENT_ITEM requirementItem;
    }
}