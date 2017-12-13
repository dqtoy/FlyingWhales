using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class CharacterSetup {
        public string fileName;
        public CharacterClass characterClass;
        public RaceSetting raceSetting;
    }
}