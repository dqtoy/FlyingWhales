using UnityEngine;
using System.Collections;

namespace ECS {
    public interface ISkill {
        SKILL_TYPE _skillType { get; set; }
        int activationWeight { get; set; }
        int accuracy { get; set; }

    }
}

