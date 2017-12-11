using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class CharacterClass : EntityComponent {
        public string className;
        public List<Skill> skills;
        public int actRate;
        public int strGain;
        public int intGain;
        public int agiGain;
        public int hpGain;
        public int dodgeRate;
        public int parryRate;
        public int blockRate;

        public CharacterClass() {
            skills = new List<Skill>();
        }
    }
}

