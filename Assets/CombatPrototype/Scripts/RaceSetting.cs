using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class RaceSetting {
        public RACE race;
        public List<BodyPart> bodyParts;
        public int baseStr;
        public int baseInt;
        public int baseAgi;
        public int baseHP;
        public int strGain;
        public int intGain;
        public int agiGain;
        public int hpGain;

        public RaceSetting(RACE race) {
            this.race = race;
            bodyParts = new List<BodyPart>();
        }
    }
}