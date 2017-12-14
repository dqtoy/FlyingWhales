using UnityEngine;
using System.Collections;

namespace ECS{
	public class CharacterSummary : MonoBehaviour {
		//public CharacterSummary Instance;

		public UILabel basicInfoLbl;
		public UILabel classInfoLbl;
		public UILabel bodyPartsInfoLbl;

		//void Awake(){
		//	Instance = this;
		//}

		public void UpdateCharacterSummary(Character character){
            basicInfoLbl.text = string.Empty;
            basicInfoLbl.text += "[b]Name:[/b] " + character.name + "\n";
            basicInfoLbl.text += "[b]Class:[/b] " + character.characterClass.className + "\n";
            basicInfoLbl.text += "[b]Race:[/b] " + character.raceSetting.race.ToString() + "\n";
            basicInfoLbl.text += "[b]HP:[/b] " + character.currentHP.ToString() + "/" + character.maxHP.ToString() + "\n";
            basicInfoLbl.text += "[b]Level:[/b] " + character.level.ToString() + "\n";
            basicInfoLbl.text += "[b]Strength:[/b] " + character.strength.ToString() + "\n";
            basicInfoLbl.text += "[b]Intelligence:[/b] " + character.intelligence.ToString() + "\n";
            basicInfoLbl.text += "[b]Agility:[/b] " + character.agility.ToString() + "\n";
            basicInfoLbl.text += "[b]Row:[/b] " + character.currentRow.ToString() + "\n";
            basicInfoLbl.text += "[b]Is Dead?:[/b] " + character.isDead.ToString() + "\n";

            classInfoLbl.text = string.Empty;
            classInfoLbl.text += "[b]Class:[/b] " + character.characterClass.className + "\n";
            classInfoLbl.text += "[b]Skills:[/b] ";
            for (int i = 0; i < character.characterClass.skills.Count; i++) {
                Skill currSkill = character.characterClass.skills[i];
                classInfoLbl.text += currSkill.skillName.ToString();
                if(i + 1 < character.characterClass.skills.Count) {
                    classInfoLbl.text += ", ";
                }
            }
            classInfoLbl.text += "\n";
            classInfoLbl.text += "[b]Act Rate:[/b] " + character.characterClass.actRate.ToString() + "\n";
            classInfoLbl.text += "[b]Strength Gain:[/b] " + character.characterClass.strGain.ToString() + "\n";
            classInfoLbl.text += "[b]Intelligence Gain:[/b] " + character.characterClass.intGain.ToString() + "\n";
            classInfoLbl.text += "[b]Agility Gain:[/b] " + character.characterClass.agiGain.ToString() + "\n";
            classInfoLbl.text += "[b]HP Gain:[/b] " + character.characterClass.hpGain.ToString() + "\n";
            classInfoLbl.text += "[b]Dodge Rate:[/b] " + character.characterClass.dodgeRate.ToString() + "\n";
            classInfoLbl.text += "[b]Parry Rate:[/b] " + character.characterClass.parryRate.ToString() + "\n";
            classInfoLbl.text += "[b]Block Rate:[/b] " + character.characterClass.blockRate.ToString();

            bodyPartsInfoLbl.text = string.Empty;
            bodyPartsInfoLbl.text += "[b]Body Parts:[/b]";
            for (int i = 0; i < character.bodyParts.Count; i++) {
                BodyPart currBodyPart = character.bodyParts[i];
                bodyPartsInfoLbl.text += "\n";
                for (int j = 0; j < currBodyPart.attributes.Count; j++) {
                    IBodyPart.ATTRIBUTE currAttribute = currBodyPart.attributes[j];
                    bodyPartsInfoLbl.text += Utilities.NormalizeString(currAttribute.ToString()) + " ";
                }
                bodyPartsInfoLbl.text += Utilities.NormalizeString(currBodyPart.bodyPart.ToString()) + "(" + currBodyPart.importance.ToString() + ")";
                for (int j = 0; j < currBodyPart.statusEffects.Count; j++) {
					STATUS_EFFECT currStatus = currBodyPart.statusEffects[j];
                    bodyPartsInfoLbl.text += "[" + currStatus.ToString() + "]";
                }

                for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
                    SecondaryBodyPart otherBodyPart = currBodyPart.secondaryBodyParts[j];
                    bodyPartsInfoLbl.text += "\n    -";
                    for (int k = 0; k < otherBodyPart.attributes.Count; k++) {
                        IBodyPart.ATTRIBUTE currAttribute = otherBodyPart.attributes[k];
                        bodyPartsInfoLbl.text += Utilities.NormalizeString(currAttribute.ToString()) + " ";
                    }
                    bodyPartsInfoLbl.text += Utilities.NormalizeString(otherBodyPart.bodyPart.ToString()) + "(" + currBodyPart.importance.ToString() + ")";
                    for (int k = 0; k < otherBodyPart.statusEffects.Count; k++) {
						STATUS_EFFECT currStatus = otherBodyPart.statusEffects[k];
                        bodyPartsInfoLbl.text += "[" + currStatus.ToString() + "]";
                    }
                }
            }
        }

//		private void BasicInfo(Character character){
//			summaryLbl 
//		}

	}
}

