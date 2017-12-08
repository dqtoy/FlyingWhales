using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class CharacterClass : EntityComponent {
        [SerializeField] protected string className;
        [SerializeField] protected Skill[] skills;
    }
}

