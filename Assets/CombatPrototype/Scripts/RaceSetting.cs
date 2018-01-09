using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ECS {
    [System.Serializable]
    public class RaceSetting {
        public RACE race;
        public List<BodyPart> bodyParts;
        public int baseStr;
        public int baseInt;
        public int baseAgi;
        public int baseHP;

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
            return newRaceSetting;
        }

//		private void ConstructBodyPartSkills(){
//			this.skills = new List<Skill> ();
//			string path = "Assets/CombatPrototype/Data/Skills/ATTACK/";
//			foreach (string file in Directory.GetFiles(path, "*.json")) {
//				AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill> (System.IO.File.ReadAllText (file));
//				if(attackSkill.skillCategory == SKILL_CATEGORY.BODY_PART){
//					this.skills.Add (attackSkill);
//				}
//			}
//			for (int i = 0; i < this.bodyParts.Count; i++) {
//				BodyPart currBodyPart = this.bodyParts [i];
//				for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
//
//				}
//			}
//		}
//
//		private void AddBodyPartSkill(IBodyPart bodyPart){
//			if(bodyPart.HasAttribute(IBodyPart.ATTRIBUTE.CAN_PUNCH_NO_WEAPON)){
//				AttackSkill punch = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText("Assets/CombatPrototype/Data/Skills/ATTACK/Punch.json"));
//				this.skills.Add (punch);
//			}
//			if(bodyPart.HasAttribute(IBodyPart.ATTRIBUTE.CAN_PUNCH_NO_WEAPON)){
//				AttackSkill punch = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText("Assets/CombatPrototype/Data/Skills/ATTACK/Punch.json"));
//				this.skills.Add (punch);
//			}
//		}
    }
}