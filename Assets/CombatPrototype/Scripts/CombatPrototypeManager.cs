using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS {
    public class CombatPrototypeManager : MonoBehaviour {

        public static CombatPrototypeManager Instance = null;

        public CharacterSetup[] baseCharacters;
		public Color[] characterColors;
//		public AttributeSkill[] attributeSkills;
		public Dictionary<WEAPON_TYPE, List<Skill>> weaponTypeSkills = new Dictionary<WEAPON_TYPE, List<Skill>> ();

		private List<Color> unusedColors = new List<Color>();
		private List<Color> usedColors = new List<Color>();
		public CombatPrototype combat;

        private void Awake() {
            Instance = this;
        }
		internal void Initialize(){
			ConstructBaseCharacters();
			ConstructCharacterColors ();
//			ConstructAttributeSkills ();
			NewCombat ();
		}
        private void ConstructBaseCharacters() {
            string path = "Assets/CombatPrototype/Data/CharacterSetups/";
            string[] baseCharacterJsons = System.IO.Directory.GetFiles(path, "*.json");
            baseCharacters = new CharacterSetup[baseCharacterJsons.Length];
            for (int i = 0; i < baseCharacterJsons.Length; i++) {
                string file = baseCharacterJsons[i];
                string dataAsJson = System.IO.File.ReadAllText(file);
                CharacterSetup charSetup = JsonUtility.FromJson<CharacterSetup>(dataAsJson);
                baseCharacters[i] = charSetup;
            }
        }

        /*
         * Create a new character given a base character setup.
         * */
        internal ECS.Character CreateNewCharacter(CharacterSetup baseCharacter) {
            return new ECS.Character(baseCharacter);
        }

		private void ConstructCharacterColors(){
			unusedColors = characterColors.ToList (); 
		}

		internal Color UseRandomCharacterColor(){
			Color chosenColor = Color.black;
			if(unusedColors.Count > 0){
				chosenColor = unusedColors [UnityEngine.Random.Range (0, unusedColors.Count)];
				unusedColors.Remove (chosenColor);
				usedColors.Add (chosenColor);
			}else{
				if(characterColors != null && characterColors.Length > 0){
					chosenColor = characterColors [UnityEngine.Random.Range (0, characterColors.Length)];
				}
			}
			return chosenColor;
		}

		internal void ReturnCharacterColorToPool(Color color){
			if(usedColors.Remove(color)){
				unusedColors.Add (color);
			}
		}

//		private void ConstructAttributeSkills(){
//			string path = "Assets/CombatPrototype/Data/AttributeSkills/";
//			string[] attributeSkillsJson = System.IO.Directory.GetFiles(path, "*.json");
//			attributeSkills = new AttributeSkill[attributeSkillsJson.Length];
//			for (int i = 0; i < attributeSkillsJson.Length; i++) {
//				string file = attributeSkillsJson[i];
//				string dataAsJson = System.IO.File.ReadAllText(file);
//				AttributeSkill attSkill = JsonUtility.FromJson<AttributeSkill>(dataAsJson);
//				attSkill.ConstructAttributeSkillsList ();
//				attributeSkills[i] = attSkill;
//			}
//		}
		internal CharacterSetup GetBaseCharacterSetupBasedOnClass(string className){
			for (int i = 0; i < this.baseCharacters.Length; i++) {
                CharacterSetup currBase = this.baseCharacters[i];
                if (currBase.characterClassName.ToLower() == className.ToLower()){
					return currBase;
				}
			}
			return null;
		}

        internal CharacterSetup GetBaseCharacterSetup(string className, RACE race) {
            for (int i = 0; i < this.baseCharacters.Length; i++) {
                CharacterSetup currBase = this.baseCharacters[i];
                if (currBase.characterClassName.ToLower() == className.ToLower() && currBase.raceSetting.race == race) {
                    return currBase;
                }
            }
            return null;
        }

		internal CharacterSetup GetBaseCharacterSetup(string fileName) {
			for (int i = 0; i < this.baseCharacters.Length; i++) {
				CharacterSetup currBase = this.baseCharacters[i];
				if (currBase.fileName == fileName) {
					return currBase;
				}
			}
			return null;
		}

		internal void NewCombat(){
			this.combat = new CombatPrototype (null, null, null);
		}
		public void StartCombat(){
			this.combat.CombatSimulation ();
		}
		public void CombatResults(CombatPrototype combat){
			for (int i = 0; i < combat.deadCharacters.Count; i++) {
                Character currDeadCharacter = combat.deadCharacters[i];
                currDeadCharacter.Death (combat.GetOpposingCharacters(currDeadCharacter));
			}

			//Prisoner or Leave to Die
			List<ECS.Character> winningCharacters = null;
			int leaveToDieWeight = 100;
			if(combat.winningSide == SIDES.A){
				if(combat.charactersSideA[0].faction == null){
					leaveToDieWeight += 200;
				}
				winningCharacters = combat.charactersSideA;
			}else{
				if(combat.charactersSideB[0].faction == null){
					leaveToDieWeight += 200;
				}
				winningCharacters = combat.charactersSideB;
			}

			if(combat.faintedCharacters.Count > 0){
				WeightedDictionary<string> prisonWeights = new WeightedDictionary<string>();
				if(!winningCharacters[0].doesNotTakePrisoners){
					int prisonerWeight = 50;
					prisonWeights.AddElement ("prison", prisonerWeight);
				}
				prisonWeights.AddElement ("leave", leaveToDieWeight);
				string pickedWeight = prisonWeights.PickRandomElementGivenWeights ();

				if(pickedWeight == "prison"){
					for (int i = 0; i < combat.faintedCharacters.Count; i++) {
                        ECS.Character currFaintedChar = combat.faintedCharacters[i];
                        if (currFaintedChar.currentSide != combat.winningSide){
							if(!currFaintedChar.cannotBeTakenAsPrisoner){
								currFaintedChar.Faint ();
								//if the currFaintedChar has a party, and it is not yet disbanded
								if (currFaintedChar.party != null && !currFaintedChar.party.isDisbanded) {
									//Check if he/she is the party leader
									if (currFaintedChar.party.IsCharacterLeaderOfParty(currFaintedChar)) {
										//if he/she is, disband the party
										currFaintedChar.party.DisbandParty();
									}
								}
								winningCharacters[0].AddPrisoner(currFaintedChar);
							}else{
								if(combat.location != null && combat.location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
									BaseLandmark landmark = combat.location as BaseLandmark;
									//landmark.AddHistory (currFaintedChar.name + " is left to die.");
								}
								currFaintedChar.Death(combat.GetOpposingCharacters(currFaintedChar));
							}
						}else{
                            currFaintedChar.SetHP(1);
						}
					}
				}else{
					for (int i = 0; i < combat.faintedCharacters.Count; i++) {
                        ECS.Character currFaintedChar = combat.faintedCharacters[i];
                        if (currFaintedChar.currentSide != combat.winningSide){
							if(combat.location != null && combat.location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
								BaseLandmark landmark = combat.location as BaseLandmark;
								//landmark.AddHistory (currFaintedChar.name + " is left to die.");
							}
                            currFaintedChar.Death(combat.GetOpposingCharacters(currFaintedChar));
						}else{
                            currFaintedChar.SetHP(1);
						}
					}
				}
			}

			//Check prisoners of defeated party or character
			if(combat.losingSide == SIDES.A && combat.sideAPrisoners != null){
				CheckDefeatedPartyPrisoners (winningCharacters, combat.sideAPrisoners);
			}else if(combat.losingSide == SIDES.B && combat.sideBPrisoners != null){
				CheckDefeatedPartyPrisoners (winningCharacters, combat.sideBPrisoners);
			}

			for (int i = 0; i < combat.fledCharacters.Count; i++) {
                ECS.Character currFleeCharacter = combat.fledCharacters[i];
                //if the current character is a follower, check if his/her party leader also fled
                if (currFleeCharacter.isFollower) {
                    //if they did, keep the current character in the party
                    if (!combat.fledCharacters.Contains(currFleeCharacter.party.partyLeader)) {
                        //if they did not, remove the current character from the party and add him/her as a civilian in the nearest settlement of his/her faction
                        currFleeCharacter.party.RemovePartyMember(currFleeCharacter, true);
                        Settlement nearestSettlement = currFleeCharacter.GetNearestSettlementFromFaction();
                        nearestSettlement.AdjustCivilians(currFleeCharacter.raceSetting.race, 1);
                    }
                }

				//if(currFleeCharacter.currentSide == combat.losingSide){
    //                //the current character is part of the losing side
				//	if(currFleeCharacter.party != null){
				//		combat.fledCharacters [i].party.SetIsDefeated (false);
				//		combat.fledCharacters [i].party.GoBackToQuestGiver(TASK_STATUS.CANCEL);
				//		break;
				//	}else{
				//		combat.fledCharacters [i].SetIsDefeated (false);
				//		combat.fledCharacters [i].GoToNearestNonHostileSettlement (() => combat.fledCharacters [i].DetermineAction());
				//	}
				//}
			}
		}

		private void CheckDefeatedPartyPrisoners(List<ECS.Character> winningCharacters, List<ECS.Character> prisoners){
			WeightedDictionary<string> weights = new WeightedDictionary<string> ();
			string pickedWeight = string.Empty;
			int takePrisonerWeight = 50;
			int releaseWeight = 100;
			int killWeight = 10;
			if(winningCharacters[0].party != null){
				if(winningCharacters[0].party.partyLeader.HasTrait(TRAIT.RUTHLESS)){
					killWeight += 500;
				}
				if(winningCharacters[0].party.partyLeader.HasTrait(TRAIT.BENEVOLENT)){
					releaseWeight += 500;
				}
				if(winningCharacters[0].party.partyLeader.HasTrait(TRAIT.PACIFIST)){
					killWeight -= 100;
					if(killWeight < 0){
						killWeight = 0;
					}
				}
			}else{
				if(winningCharacters[0].HasTrait(TRAIT.RUTHLESS)){
					killWeight += 500;
				}
				if(winningCharacters[0].HasTrait(TRAIT.BENEVOLENT)){
					releaseWeight += 500;
				}
				if(winningCharacters[0].HasTrait(TRAIT.PACIFIST)){
					killWeight -= 100;
					if(killWeight < 0){
						killWeight = 0;
					}
				}
			}

			weights.AddElement ("prisoner", takePrisonerWeight);
			weights.AddElement ("release", releaseWeight);

			while(prisoners.Count > 0) {
				if(prisoners[0].faction != null){
					if(winningCharacters[0].faction != null){
						if (prisoners [0].faction.id == winningCharacters [0].faction.id) {
							prisoners [0].ReleasePrisoner ();
							continue;
						} else {
							FactionRelationship fr = prisoners [0].faction.GetRelationshipWith (winningCharacters [0].faction);
							if(fr != null && fr.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
								killWeight += 200;
							}
						}
					}else{
						killWeight += 200;
					}
				}else{
					killWeight += 200;
				}

				if (winningCharacters [0].party != null){
					if (winningCharacters [0].party.partyLeader.raceSetting.race != prisoners [0].raceSetting.race && winningCharacters [0].party.partyLeader.HasTrait (TRAIT.RACIST)) {
						killWeight += 100;
					}
				}else{
					if (winningCharacters [0].raceSetting.race != prisoners [0].raceSetting.race && winningCharacters [0].HasTrait (TRAIT.RACIST)) {
						killWeight += 100;
					}
				}

				weights.ChangeElement ("kill", killWeight);
				pickedWeight = weights.PickRandomElementGivenWeights ();
				if(pickedWeight == "prisoner"){
					if (winningCharacters [0].party != null) {
						prisoners [0].TransferPrisoner (winningCharacters [0].party);
					}else{
						prisoners [0].TransferPrisoner (winningCharacters [0]);
					}
				}else if(pickedWeight == "kill"){
                    if (winningCharacters[0].party != null) {
                        prisoners[0].Death(winningCharacters[0].party);
                    } else {
                        prisoners[0].Death(winningCharacters[0]);
                    }
				}else if(pickedWeight == "release"){
					prisoners [0].ReleasePrisoner ();
				}
			}
		}
    }
}