using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class CharacterClass : EntityComponent {
        public string className;
        public Skill[] skills;
        public int actRate;
        public int strGain;
        public int intGain;
        public int agiGain;
        public int hpGain;
        public int dodgeRate;
        public int parryRate;
        public int blockRate;
    }
}

