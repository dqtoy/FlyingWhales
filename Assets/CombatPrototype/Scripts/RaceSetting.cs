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

        internal RaceSetting CreateNewCopy() {
            RaceSetting newRaceSetting = new RaceSetting();
            newRaceSetting.race = this.race;
            newRaceSetting.bodyParts = new List<BodyPart>();
            for (int i = 0; i < this.bodyParts.Count; i++) {
                BodyPart currBodyPart = this.bodyParts[i];
                newRaceSetting.bodyParts.Add(currBodyPart.CreateNewCopy());
            }
            newRaceSetting.baseStr = this.baseStr;
            newRaceSetting.baseInt = this.baseInt;
            newRaceSetting.baseAgi = this.baseAgi;
            newRaceSetting.baseHP = this.baseHP;
            newRaceSetting.strGain = this.strGain;
            newRaceSetting.intGain = this.intGain;
            newRaceSetting.agiGain = this.agiGain;
            newRaceSetting.hpGain = this.hpGain;

            return newRaceSetting;
        }
    }
}