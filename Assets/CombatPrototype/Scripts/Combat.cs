using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

	public class Combat : Multithread {
//		public Dictionary<SIDES, List<ECS.Character>> allCharactersAndSides;
		internal List<ECS.Character> charactersSideA;
		internal List<ECS.Character> charactersSideB;
		internal List<ECS.Character> deadCharacters;
		internal List<ECS.Character> faintedCharacters;
		internal List<ECS.Character> fledCharacters;
		internal List<ECS.Character> characterSideACopy;
		internal List<ECS.Character> characterSideBCopy;

		internal SIDES winningSide;
		internal SIDES losingSide;

		internal List<string> resultsLog;
        internal ILocation location;
		internal bool isDone;
        internal bool hasStarted;

		public Combat(ILocation location){
//			this.allCharactersAndSides = new Dictionary<SIDES, List<ECS.Character>> ();
			this.charactersSideA = new List<ECS.Character> ();
			this.charactersSideB = new List<ECS.Character> ();
            this.characterSideACopy = new List<ECS.Character>();
            this.characterSideBCopy = new List<ECS.Character>();
            this.deadCharacters = new List<ECS.Character> ();
			this.faintedCharacters = new List<ECS.Character> ();
			this.fledCharacters = new List<Character> ();
			this.location = location;
			this.isDone = false;
            this.hasStarted = false;

			this.resultsLog = new List<string> ();
//			Messenger.AddListener<ECS.Character> ("CharacterDeath", CharacterDeath);
		}

        #region ECS.Character Management
        //Add a character to a side
        internal void AddCharacter(SIDES side, ECS.Character character) {
            if(!this.charactersSideA.Contains(character) && !this.charactersSideB.Contains(character)) {
                int rowNumber = 1;
                if (side == SIDES.A) {
                    this.charactersSideA.Add(character);
                    this.characterSideACopy.Add(character);
                } else {
                    this.charactersSideB.Add(character);
                    this.characterSideBCopy.Add(character);
                    rowNumber = 5;
                }
                character.SetSide(side);
                character.currentCombat = this;
                character.SetRowNumber(rowNumber);
                character.actRate = character.agility * 5;
                if (hasStarted && !isDone) {
                    string log = character.coloredUrlName + " joins the battle on Side " + side.ToString();
                    Debug.Log(log);
                    AddCombatLog(log, side);
                }
                if (CombatPrototypeUI.Instance != null) {
                    CombatPrototypeUI.Instance.UpdateCharactersList(side);
                }
            }
        }
		internal void AddCharacters(SIDES side, List<ECS.Character> characters) {
            int rowNumber = 1;
            if (side == SIDES.A) {
				this.charactersSideA.AddRange(characters);
				this.characterSideACopy.AddRange(characters);
			} else {
				this.charactersSideB.AddRange(characters);
                this.characterSideBCopy.AddRange(characters);
                rowNumber = 5;
            }
            for (int i = 0; i < characters.Count; i++) {
				characters[i].SetSide (side);
				characters[i].currentCombat = this;
                characters[i].SetRowNumber(rowNumber);
                characters[i].actRate = characters[i].agility * 5;
                if (hasStarted && !isDone) {
                    string log = characters[i].coloredUrlName + " joins the battle on Side " + side.ToString();
                    Debug.Log(log);
                    AddCombatLog(log, side);
                }
            }
            if (CombatPrototypeUI.Instance != null){
				CombatPrototypeUI.Instance.UpdateCharactersList(side);
			}
		}
        //Remove a character from a side
        internal void RemoveCharacter(SIDES side, ECS.Character character) {
            if (side == SIDES.A) {
                this.charactersSideA.Remove(character);
            } else {
                this.charactersSideB.Remove(character);
            }
			character.currentCombat = null;
			if (CombatPrototypeUI.Instance != null) {
				CombatPrototypeUI.Instance.UpdateCharactersList (side);
			}
        }
        //Remove character without specifying a side
        internal void RemoveCharacter(ECS.Character character) {
            if (this.charactersSideA.Remove(character)) {
				character.currentCombat = null;
				if (CombatPrototypeUI.Instance != null) {
					CombatPrototypeUI.Instance.UpdateCharactersList (SIDES.A);
				}
            } else {
                this.charactersSideB.Remove(character);
				character.currentCombat = null;
				if (CombatPrototypeUI.Instance != null) {
					CombatPrototypeUI.Instance.UpdateCharactersList (SIDES.B);
				}
            }
        }
        internal List<ECS.Character> GetCharactersOnSide(SIDES side) {
            if (side == SIDES.A) {
                return charactersSideA;
            } else {
                return charactersSideB;
            }
        }
        #endregion

        #region Overrides
        public override void DoMultithread() {
            base.DoMultithread();
            CombatSimulation();
        }
        public override void FinishMultithread() {
            base.FinishMultithread();
            ReturnCombatResults();
        }
        #endregion
        public void ReturnCombatResults(){
			CombatManager.Instance.CombatResults(this);
            //if (attacker != null) {
            //    attacker.ReturnCombatResults(this);
            //}
            //if (defender != null) {
            //    defender.ReturnCombatResults(this);
            //}
		}

        //This simulates the whole combat system
		public void CombatSimulation(){
            CombatManager.Instance.StartCoroutine(CombatSimulationCoroutine());
        }

        private IEnumerator CombatSimulationCoroutine() {
            ClearCombatLogs();
            AddCombatLog("Combat starts", SIDES.A);
            hasStarted = true;
            string sideAChars = string.Empty;
            string sideBChars = string.Empty;
            for (int i = 0; i < charactersSideA.Count; i++) {
                sideAChars += charactersSideA[i].name + "\n";
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                sideBChars += charactersSideB[i].name + "\n";
            }
            Debug.Log("Starting Side A : \n" + sideAChars);
            Debug.Log("Starting Side B : \n" + sideBChars);
            Dictionary<ECS.Character, int> characterActivationWeights = new Dictionary<ECS.Character, int>();
            //SetRowNumber (this.charactersSideA, 1);
            //SetRowNumber (this.charactersSideB, 5);

            int rounds = 1;
            while (this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0) {
                Debug.Log("========== Round " + rounds.ToString() + " ==========");
                ECS.Character characterThatWillAct = GetCharacterThatWillAct(characterActivationWeights, this.charactersSideA, this.charactersSideB);
                characterThatWillAct.EnableDisableSkills(this);
                Debug.Log(characterThatWillAct.characterClass.className + " " + characterThatWillAct.name + " will act");
                Debug.Log("Available Skills: ");
                for (int i = 0; i < characterThatWillAct.skills.Count; i++) {
                    Skill currSkill = characterThatWillAct.skills[i];
                    if (currSkill.isEnabled) {
                        Debug.Log(currSkill.skillName);
                    }
                }

                Skill skillToUse = GetSkillToUse(characterThatWillAct);
                if (skillToUse != null) {
                    Debug.Log(characterThatWillAct.name + " decides to use " + skillToUse.skillName);
                    characterThatWillAct.CureStatusEffects();
                    ECS.Character targetCharacter = GetTargetCharacter(characterThatWillAct, skillToUse);
                    Debug.Log(characterThatWillAct.name + " decides to use it on " + targetCharacter.name);
                    DoSkill(skillToUse, characterThatWillAct, targetCharacter);
                }
                if (CombatPrototypeUI.Instance != null) {
                    CombatPrototypeUI.Instance.UpdateCharacterSummary();
                }
                Debug.Log("========== End Round " + rounds.ToString() + " ==========");
                rounds++;
                yield return new WaitWhile(() => GameManager.Instance.isPaused == true);
                yield return new WaitForSeconds(CombatManager.Instance.updateIntervals);
            }
            if (this.charactersSideA.Count > 0) {
                winningSide = SIDES.A;
                losingSide = SIDES.B;
            } else {
                winningSide = SIDES.B;
                losingSide = SIDES.A;
            }
            AddCombatLog("Combat Ends", SIDES.A);
            sideAChars = string.Empty;
            sideBChars = string.Empty;
            for (int i = 0; i < charactersSideA.Count; i++) {
                sideAChars += charactersSideA[i].name + "\n";
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                sideBChars += charactersSideB[i].name + "\n";
            }
            Debug.Log("Side A : \n" + sideAChars);
            Debug.Log("Side B : \n" + sideBChars);
            isDone = true;
            ReturnCombatResults();
        }

		//Set row number to a list of characters
		private void SetRowNumber(List<ECS.Character> characters, int rowNumber){
			for (int i = 0; i < characters.Count; i++) {
				characters [i].SetRowNumber(rowNumber);
			}
		}

		//Return a character that will act from a pool of characters based on their act rate
		private ECS.Character GetCharacterThatWillAct(Dictionary<ECS.Character, int> characterActivationWeights, List<ECS.Character> charactersSideA, List<ECS.Character> charactersSideB){
			characterActivationWeights.Clear();
            for (int i = 0; i < charactersSideA.Count; i++) {
                characterActivationWeights.Add(charactersSideA[i], charactersSideA[i].actRate);
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                characterActivationWeights.Add(charactersSideB[i], charactersSideB[i].actRate);
            }

            ECS.Character chosenCharacter = Utilities.PickRandomElementWithWeights<ECS.Character>(characterActivationWeights);
			foreach (ECS.Character character in characterActivationWeights.Keys) {
				character.actRate += character.baseAgility;
			}
			chosenCharacter.actRate = chosenCharacter.agility * 5;
			return chosenCharacter;
		}

		//Get a random character from the opposite side to be the target
		private ECS.Character GetTargetCharacter(ECS.Character sourceCharacter, Skill skill){
			List<ECS.Character> possibleTargets = new List<ECS.Character>();
			if (skill is AttackSkill) {
				List<ECS.Character> oppositeTargets = this.charactersSideB;
				if(sourceCharacter.currentSide == SIDES.B){
					oppositeTargets = this.charactersSideA;
				}

				int chance = Utilities.rng.Next(0, 100);
				if(sourceCharacter.HasTag(CHARACTER_TAG.MILD_PSYTOXIN)){
					if(chance < 10){
						if(sourceCharacter.currentSide == SIDES.A){
							oppositeTargets = this.charactersSideA;
						}else{
							oppositeTargets = this.charactersSideB;
						}
					}
				}else if(sourceCharacter.HasTag(CHARACTER_TAG.MODERATE_PSYTOXIN)){
					if(chance < 20){
						if(sourceCharacter.currentSide == SIDES.A){
							oppositeTargets = this.charactersSideA;
						}else{
							oppositeTargets = this.charactersSideB;
						}
					}
				}

				for (int i = 0; i < oppositeTargets.Count; i++) {
					ECS.Character targetCharacter = oppositeTargets [i];
					int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
					if (skill.range >= rowDistance) {
						possibleTargets.Add (targetCharacter);
					}
				}
			} else if (skill is HealSkill) {
				List<ECS.Character> sameTargets = this.charactersSideB;
				if(sourceCharacter.currentSide == SIDES.A){
					sameTargets = this.charactersSideA;
				}

				int chance = Utilities.rng.Next (0, 100);
				if(sourceCharacter.HasTag(CHARACTER_TAG.MILD_PSYTOXIN)){
					if(chance < 10){
						if(sourceCharacter.currentSide == SIDES.B){
							sameTargets = this.charactersSideA;
						}else{
							sameTargets = this.charactersSideB;
						}
					}
				}else if(sourceCharacter.HasTag(CHARACTER_TAG.MODERATE_PSYTOXIN)){
					if(chance < 20){
						if(sourceCharacter.currentSide == SIDES.B){
							sameTargets = this.charactersSideA;
						}else{
							sameTargets = this.charactersSideB;
						}
					}
				}

				for (int i = 0; i < sameTargets.Count; i++) {
					ECS.Character targetCharacter = sameTargets [i];
					int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
					if (skill.range >= rowDistance) {
						possibleTargets.Add (targetCharacter);
					}
				}
//				if (sourceCharacter.currentSide == SIDES.A) {
//					for (int i = 0; i < this.charactersSideA.Count; i++) {
//						ECS.Character targetCharacter = this.charactersSideA [i];
//						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
//						if (skill.range >= rowDistance) {
//							possibleTargets.Add (targetCharacter);
//						}
//					}
//				} else {
//					for (int i = 0; i < this.charactersSideB.Count; i++) {
//						ECS.Character targetCharacter = this.charactersSideB [i];
//						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
//						if (skill.range >= rowDistance) {
//							possibleTargets.Add (targetCharacter);
//						}
//					}
//				}
			}else{
				possibleTargets.Add (sourceCharacter);
			}

			return possibleTargets [Utilities.rng.Next (0, possibleTargets.Count)];
		}

		//Get Skill that the character will use based on activation weights, target character must be within skill range
		private Skill GetSkillToUse(ECS.Character sourceCharacter){
			Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int> ();
			Dictionary<object, int> categoryActivationWeights = new Dictionary<object, int> ();

			//First step: pick from general skills or body part skill or weapon skill
			int bodyPartWeight = 10;
			if(sourceCharacter.HasActivatableWeaponSkill()){
				categoryActivationWeights.Add("weapon", 100);
			}else{
				bodyPartWeight += 100;
			}
			if (sourceCharacter.HasActivatableBodyPartSkill ()) {
				categoryActivationWeights.Add ("bodypart", bodyPartWeight);
			}

			for (int i = 0; i < sourceCharacter.skills.Count; i++) {
				Skill skill = sourceCharacter.skills [i];
				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.GENERAL){
					int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
					if(skill is MoveSkill && HasTargetInRangeForSkill(SKILL_TYPE.ATTACK, sourceCharacter)){
						activationWeight /= 2;
					}else if(skill is FleeSkill){
						activationWeight = 50 - (sourceCharacter.currentHP / ((int)(sourceCharacter.maxHP * 0.01f)));
						if(activationWeight == 0){
							activationWeight = 1;
						}
						activationWeight *= 2;
					}
					if(activationWeight > 0){
						categoryActivationWeights.Add (skill, activationWeight);
					}
				}
			}
			if(categoryActivationWeights.Count > 0){
				object chosenObject = Utilities.PickRandomElementWithWeights<object> (categoryActivationWeights);
				if(chosenObject is string){
					string chosenCategory = chosenObject.ToString ();
					if(chosenCategory == "bodypart"){
						for (int i = 0; i < sourceCharacter.skills.Count; i++) {
							Skill skill = sourceCharacter.skills [i];
							if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.BODY_PART && HasTargetInRangeForSkill(skill, sourceCharacter)){
								int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
								skillActivationWeights.Add (skill, activationWeight);
							}
						}
					}else if(chosenCategory == "weapon"){
						for (int i = 0; i < sourceCharacter.skills.Count; i++) {
							Skill skill = sourceCharacter.skills [i];
							if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.WEAPON && HasTargetInRangeForSkill(skill, sourceCharacter)){
								int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
								skillActivationWeights.Add (skill, activationWeight);
							}
						}
					}
					if(skillActivationWeights.Count > 0){
						Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill> (skillActivationWeights);
						return chosenSkill;
					}
				}else{
					return (Skill)chosenObject;
				}
			}
			return null;
		}

		//Returns activation weight of a certain skill that is already modified
		private int GetActivationWeightOfSkill(ECS.Character sourceCharacter, Skill skill){
			int activationWeight = skill.activationWeight;
			//if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.CURRENT_HEALTH){
			//	activationWeight *= ((int)(((float)sourceCharacter.currentHP / (float)sourceCharacter.maxHP) * 100f));
			//}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.MISSING_HEALTH){
			//	int missingHealth = sourceCharacter.maxHP - sourceCharacter.currentHP;
			//	int weight = (int)(((float)missingHealth / (float)sourceCharacter.maxHP) * 100f);
			//	activationWeight *=  (weight > 0f ? weight : 1);
			//}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.ALLY_MISSING_HEALTH){
			//	int highestMissingHealth = 0;
			//	ECS.Character chosenCharacter = null;
			//	if(sourceCharacter.currentSide == SIDES.A){
			//		for (int j = 0; j < charactersSideA.Count; j++) {
			//			ECS.Character character = charactersSideA [j];
			//			int missingHealth = character.maxHP - character.currentHP;
			//			if(chosenCharacter == null){
			//				highestMissingHealth = missingHealth;
			//				chosenCharacter = character;
			//			}else{
			//				if(missingHealth > highestMissingHealth){
			//					highestMissingHealth = missingHealth;
			//					chosenCharacter = character;
			//				}
			//			}
			//		}
			//	}else{
			//		for (int j = 0; j < charactersSideB.Count; j++) {
			//			ECS.Character character = charactersSideB [j];
			//			int missingHealth = character.maxHP - character.currentHP;
			//			if(chosenCharacter == null){
			//				highestMissingHealth = missingHealth;
			//				chosenCharacter = character;
			//			}else{
			//				if(missingHealth > highestMissingHealth){
			//					highestMissingHealth = missingHealth;
			//					chosenCharacter = character;
			//				}
			//			}
			//		}
			//	}
			//	if(chosenCharacter != null){
			//		int weight = (int)((((float)highestMissingHealth / (float)chosenCharacter.maxHP) * 100f) * 2f);
			//		activationWeight *= (weight > 0f ? weight : 1);
			//	}
			//}
			return activationWeight;
		}
		//Check if there are targets in range for the specific skill so that the character can know if the skill can be activated 
		internal bool HasTargetInRangeForSkill(Skill skill, ECS.Character sourceCharacter){
			if(skill is AttackSkill){
				if(sourceCharacter.currentSide == SIDES.A){
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						ECS.Character targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if(skill.range >= rowDistance){
							return true;
						}
					}
				}else{
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						ECS.Character targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if(skill.range >= rowDistance){
							return true;
						}
					}
				}
				return false;
			}else{
				return true;
			}

		}

		internal bool HasTargetInRangeForSkill(SKILL_TYPE skillType, ECS.Character sourceCharacter){
			if(skillType == SKILL_TYPE.ATTACK){
				for (int i = 0; i < sourceCharacter.skills.Count; i++) {
					Skill skill = sourceCharacter.skills [i];
					if(skill is AttackSkill){
                        return HasTargetInRangeForSkill(skill, sourceCharacter);
					}
				}
                for (int i = 0; i < sourceCharacter.level; i++) {
                    for (int j = 0; j < sourceCharacter.characterClass.skillsPerLevel[i].Length; j++) {
                        Skill skill = sourceCharacter.characterClass.skillsPerLevel[i][j];
                        if (skill is AttackSkill) {
                            return HasTargetInRangeForSkill(skill, sourceCharacter);
                        }
                    }
                }
			}
			return true;
		}
		//Returns the row distance/difference of two characters
		private int GetRowDistanceBetweenTwoCharacters(ECS.Character sourceCharacter, ECS.Character targetCharacter){
			int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
			if(distance < 0){
				distance *= -1;
			}
			return distance;
		}
		//ECS.Character will do the skill specified, but its success will be determined by the skill's accuracy
		private void DoSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
            //If skill is attack, reduce sp
            if (skill.skillType == SKILL_TYPE.ATTACK) {
                AttackSkill attackSkill = skill as AttackSkill;
                sourceCharacter.AdjustSP(-attackSkill.spCost);
            }
            SuccessfulSkill(skill, sourceCharacter, targetCharacter);

   //         int chance = Utilities.rng.Next (0,100);
			//if(chance < skill.accuracy){
			//	//Successful
			//	SuccessfulSkill(skill, sourceCharacter, targetCharacter);
			//}else{
			//	//Fail
			//	FailedSkill(skill, sourceCharacter, targetCharacter);
			//}
		}

		//Go here if skill is accurate and is successful
		private void SuccessfulSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			if (skill is AttackSkill) {
				AttackSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is HealSkill) {
				HealSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is FleeSkill) {
				FleeSkill (sourceCharacter, targetCharacter);
			} else if (skill is ObtainSkill) {
				ObtainItemSkill (sourceCharacter, targetCharacter);
			} else if (skill is MoveSkill) {
				MoveSkill (skill, sourceCharacter, targetCharacter);
			}
		}

		//Skill is not accurate and therefore has failed to execute
		private void FailedSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			//TODO: What happens when a skill has failed?
			if(skill is FleeSkill){
				AddCombatLog(sourceCharacter.coloredUrlName + " tried to flee but got tripped over and fell down!", sourceCharacter.currentSide);
//				CounterAttack (targetCharacter);
			}else if(skill is AttackSkill){
				AddCombatLog (sourceCharacter.coloredUrlName + " tried to " + skill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " but missed!", sourceCharacter.currentSide);
			}else if(skill is HealSkill){
				if(sourceCharacter == targetCharacter){
					AddCombatLog (sourceCharacter.coloredUrlName + " tried to use " + skill.skillName.ToLower() + " to heal himself/herself but it is already expired!", sourceCharacter.currentSide);
				}else{
					AddCombatLog (sourceCharacter.coloredUrlName + " tried to use " + skill.skillName.ToLower() + " to heal " + targetCharacter.coloredUrlName + " but it is already expired!", sourceCharacter.currentSide);
				}
			}
		}

		//Get DEFEND_TYPE for the attack skill, if DEFEND_TYPE is NONE, then target character has not defend successfully, therefore, the target character will be damaged
		//private DEFEND_TYPE CanTargetCharacterDefend(ECS.Character sourceCharacter, ECS.Character targetCharacter){
		//	if(sourceCharacter.HasStatusEffect(STATUS_EFFECT.CONFUSED)){
		//		return DEFEND_TYPE.NONE;
		//	}
		//	int dodgeChance = Utilities.rng.Next (0, 100);
		//	if(dodgeChance < targetCharacter.dodgeRate){
		//		return DEFEND_TYPE.DODGE;
		//	}else{
		//		int parryChance = Utilities.rng.Next (0, 100);
		//		if(parryChance < targetCharacter.parryRate){
		//			return DEFEND_TYPE.PARRY;
		//		}else{
		//			int blockChance = Utilities.rng.Next (0, 100);
		//			if(blockChance < targetCharacter.blockRate){
		//				return DEFEND_TYPE.BLOCK;
		//			}else{
		//				return DEFEND_TYPE.NONE;
		//			}
		//		}
		//	}
		//}

		private void CounterAttack(ECS.Character character){
			//TODO: Counter attack
			AddCombatLog (character.coloredUrlName + " counterattacked!", character.currentSide);
		}

		private void InstantDeath(ECS.Character character){
			character.FaintOrDeath();
		}

		#region Attack Skill
		private void AttackSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			AttackSkill attackSkill = skill as AttackSkill;
            HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter); //, attackSkill.weapon
   //         DEFEND_TYPE defendType = CanTargetCharacterDefend (sourceCharacter, targetCharacter);
			//if(defendType == DEFEND_TYPE.NONE){
			//	//Successfully hits the target character
			//	HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter); //, attackSkill.weapon
			//}else{
			//	//Target character has defend successfully and will roll for counter attack
			//	if(defendType == DEFEND_TYPE.DODGE){
			//		AddCombatLog(targetCharacter.coloredUrlName + " dodged " + sourceCharacter.coloredUrlName + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
			//	} else if(defendType == DEFEND_TYPE.BLOCK){
			//		AddCombatLog(targetCharacter.coloredUrlName + " blocked " + sourceCharacter.coloredUrlName + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
			//	} else if(defendType == DEFEND_TYPE.PARRY){
			//		AddCombatLog(targetCharacter.coloredUrlName + " parried " + sourceCharacter.coloredUrlName + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
			//	}
			//	CounterAttack(targetCharacter);
			//}
		}
			
		//Hits the target with an attack skill
		private void HitTargetCharacter(AttackSkill attackSkill, ECS.Character sourceCharacter, ECS.Character targetCharacter, Weapon weapon = null){
			//Total Damage = [Weapon Power + (Int or Str)] - [Base Damage Mitigation] - [Bonus Attack Type Mitigation] + [Bonus Attack Type Weakness]
			if(sourceCharacter.HasStatusEffect(STATUS_EFFECT.CONFUSED)){
				targetCharacter = sourceCharacter;
				string confusedLog = sourceCharacter.coloredUrlName + " is so confused that " + (sourceCharacter.gender == GENDER.MALE ? "he" : "she") + " targeted " 
					+ (sourceCharacter.gender == GENDER.MALE ? "himself" : "herself");
				AddCombatLog (confusedLog, sourceCharacter.currentSide);
			}
            string log = string.Empty;
            float weaponPower = 0f;
            float damageRange = 5f;
            int statMod = sourceCharacter.strength;
            int def = targetCharacter.GetPDef(sourceCharacter);
            if(attackSkill.attackCategory == ATTACK_CATEGORY.MAGICAL) {
                statMod = sourceCharacter.intelligence;
                def = targetCharacter.GetMDef(sourceCharacter);
            }
            BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
            if (chosenBodyPart == null) {
                Debug.LogError("NO MORE BODY PARTS!");
                return;
            }
            Armor armor = chosenBodyPart.GetArmor();
            log += sourceCharacter.coloredUrlName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " in the " + chosenBodyPart.name.ToLower();

            if (weapon != null) {
                damageRange = ItemManager.Instance.weaponTypeData[weapon.weaponType].damageRange;
                weaponPower = weapon.weaponPower;

                log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " " + weapon.itemName + ".";

            } else {
                log += ".";
            }
            float weaponAttack = weapon.attackPower;
            int finalAttack = GetFinalAttack(statMod, sourceCharacter.level, weaponAttack);
            int damage = (int) (finalAttack * attackSkill.power); //To be changed and add crit
            int computedDamageRange = (int) ((float) damage * (damageRange / 100f));
            int minDamageRange = damage - computedDamageRange;
            int maxDamageRange = damage + computedDamageRange;
            damage = Utilities.rng.Next((minDamageRange < 0 ? 0 : minDamageRange), maxDamageRange + 1);

            //Reduce damage by defense of target
            damage -= def;

            //Calculate elemental weakness and resistance
            //Use element of skill if it has one, if not, use weapon element instead if it has one
            ELEMENT elementUsed = ELEMENT.NONE;
            if(attackSkill.element != ELEMENT.NONE) {
                elementUsed = attackSkill.element;
            } else {
                if(weapon.element != ELEMENT.NONE) {
                    elementUsed = weapon.element;
                }
            }
            float elementalDiff = targetCharacter.elementalWeaknesses[elementUsed] - targetCharacter.elementalResistance[elementUsed];
            float elementModifier = 1f + ((elementalDiff < 0f ? 0f : elementalDiff) / 100f);

            //Calculate total damage
            damage = (int) ((float) damage * elementModifier);

            log += "(" + damage.ToString() + ")";

            DealDamageToBodyPart(attackSkill, targetCharacter, sourceCharacter, chosenBodyPart, ref log);

            AddCombatLog(log, sourceCharacter.currentSide);

            targetCharacter.AdjustHP(-damage);

            //if (attackSkill.attackType != ATTACK_TYPE.STATUS) {
            //    string log = string.Empty;
            //    float weaponPower = 0f;
            //    float damageRange = 5f;

            //    BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
            //    if (chosenBodyPart == null) {
            //        Debug.LogError("NO MORE BODY PARTS!");
            //        return;
            //    }
            //    Armor armor = chosenBodyPart.GetArmor();
            //    log += sourceCharacter.coloredUrlName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " in the " + chosenBodyPart.name.ToLower();

            //    if (weapon != null) {
            //        damageRange = weapon.damageRange;
            //        weaponPower = weapon.weaponPower;

            //        //reduce weapon durability by durability cost of skill
            //        weapon.AdjustDurability(-attackSkill.durabilityCost);
            //        log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " " + weapon.itemName + ".";

            //    } else {
            //        log += ".";
            //    }

            //    int damage = (int) (weaponPower + (attackSkill.attackType == ATTACK_TYPE.MAGIC ? sourceCharacter.intelligence : sourceCharacter.strength));
            //    int computedDamageRange = (int) ((float) damage * (damageRange / 100f));
            //    int minDamageRange = damage - computedDamageRange;
            //    int maxDamageRange = damage + computedDamageRange;
            //    damage = Utilities.rng.Next((minDamageRange < 0 ? 0 : minDamageRange), maxDamageRange + 1);

            //    if (armor != null) {
            //        if (attackSkill.attackType != ATTACK_TYPE.PIERCE) {
            //            int damageNullChance = Utilities.rng.Next(0, 100);
            //            if (damageNullChance < armor.damageNullificationChance) {
            //                log += " The attack was fully absorbed by the " + armor.itemName + ".";
            //                return;
            //            }
            //            damage -= (int) ((float) damage * (armor.baseDamageMitigation / 100f));
            //        } else {
            //            damage -= (int) ((float) damage * ((armor.baseDamageMitigation / 2f) / 100f));
            //        }
            //        if (armor.ineffectiveAttackTypes.Contains(attackSkill.attackType)) {
            //            damage -= (int) ((float) damage * 0.2f);
            //        }
            //        if (armor.effectiveAttackTypes.Contains(attackSkill.attackType)) {
            //            damage += (int) ((float) damage * 0.2f);
            //        }
            //        armor.AdjustDurability(-attackSkill.durabilityDamage);
            //    }
            //    log += "(" + damage.ToString() + ")";

            //    DealDamageToBodyPart(attackSkill, targetCharacter, sourceCharacter, chosenBodyPart, ref log);

            //    AddCombatLog(log, sourceCharacter.currentSide);

            //    targetCharacter.AdjustHP(-damage);
            //} else {
            //    string log = sourceCharacter.coloredUrlName + " used " + attackSkill.skillName.ToLower() + " on " + targetCharacter.coloredUrlName + ".";
            //    int chance = Utilities.rng.Next(0, 100);
            //    if (attackSkill.statusEffectRates != null && attackSkill.statusEffectRates.Count > 0) {
            //        for (int i = 0; i < attackSkill.statusEffectRates.Count; i++) {
            //            int value = attackSkill.statusEffectRates[i].ratePercentage;
            //            if (chance < value) {
            //                targetCharacter.AddStatusEffect(attackSkill.statusEffectRates[i].statusEffect);
            //                log += " " + StatusEffectLog(sourceCharacter, targetCharacter, attackSkill.statusEffectRates[i].statusEffect);
            //            }
            //        }
            //    }
            //    AddCombatLog(log, sourceCharacter.currentSide);
            //}
        }

		//This will select, deal damage, and apply status effect to a body part if possible 
		private void DealDamageToBodyPart(AttackSkill attackSkill, ECS.Character targetCharacter, ECS.Character sourceCharacter, BodyPart chosenBodyPart, ref string log){
			//int chance = Utilities.rng.Next (0, 100);

//			if(attackSkill.statusEffectRates != null && attackSkill.statusEffectRates.Count > 0){
//				for (int i = 0; i < attackSkill.statusEffectRates.Count; i++) {
//					int value = attackSkill.statusEffectRates[i].ratePercentage;
//					if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.INJURED){
//						if(attackSkill.attackType == ATTACK_TYPE.CRUSH){
//							value += 7;
//						}else{
//						}

//					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BLEEDING){
//						if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
//							value += 10;
//						}else{
//						}
//					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.DECAPITATED){
//						if(attackSkill.attackType == ATTACK_TYPE.SLASH){
//							value += 5;
//						}else{
//						}
//					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BURNING){
//						if(attackSkill.attackType == ATTACK_TYPE.MAGIC){
//							value += 5;
//						}
//					}

//					if(chance < value){
//						chosenBodyPart.AddStatusEffect(attackSkill.statusEffectRates[i].statusEffect);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (attackSkill.statusEffectRates[i].statusEffect);
////						AddCombatLog (log);
//					}
//				}
//			}else{
//				if (attackSkill.attackType == ATTACK_TYPE.CRUSH){
//					if(chance < 7){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.INJURED);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.INJURED);

//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							string[] predicate = new string[]{ "battered", "crippled", "mangled", "brokened" };
//							log += " " + targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + " is " + predicate[Utilities.rng.Next(0, predicate.Length)] + "!";
//						}else{
//							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
//							if(secondaryBodPart != null){
//								log += " " + targetCharacter.coloredUrlName + "'s " + secondaryBodPart.name.ToLower() + " makes a crunching noise!";
//							}
//						}

//					}
//				}else if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
//					if(chance < 10){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.BLEEDING);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BLEEDING);

//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
//							if(secondaryBodPart != null){
//								string[] adjective = new string[]{ "deep", "light", "painful", "fresh", "deadly" };
//								string[] noun = new string[]{ "gash", "wound", "lesion", "tear" };

//								log += " A " + adjective[Utilities.rng.Next(0, adjective.Length)] + " " + noun[Utilities.rng.Next(0, noun.Length)] + " forms near " + targetCharacter.coloredUrlName + "'s " + secondaryBodPart.name.ToLower() + ".";
//							}
//						}else{
//							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
//							if(secondaryBodPart != null){
//								log += " Blood erupts from " + targetCharacter.coloredUrlName + "'s " + secondaryBodPart.name.ToLower() + "!";
//							}
//						}
//					}
//				}else if(attackSkill.attackType == ATTACK_TYPE.SLASH){
//					if (chosenBodyPart.HasAttribute (IBodyPart.ATTRIBUTE.NONDECAPITATABLE)){
//						return;
//					}
//					if(chance < 5){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.DECAPITATED);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.DECAPITATED);

//						string[] verb = new string[]{ "severed", "decapitated", "sliced off", "lopped off" };
//						log += targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + " has been " + verb[Utilities.rng.Next(0, verb.Length)] + " by the attack!";

//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							log += " It drops to the floor lifelessly.";
//						}else{
//							log += " It flew away!";
//						}

//						string allWeaponDropped = string.Empty;
//						for (int i = 0; i < targetCharacter.equippedItems.Count; i++) {
//							Item item = targetCharacter.equippedItems [i];
//							if(item is Weapon){
//								Weapon weapon = (Weapon)item;
//								for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
//									if(weapon.bodyPartsAttached[j].statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
//										if(allWeaponDropped != string.Empty){
//											allWeaponDropped += ", ";
//										}
//										allWeaponDropped += item.itemName;
//										targetCharacter.ThrowItem (item);
//										break;
//									}
//								}
//							}
//						}
//						if(allWeaponDropped != string.Empty){
//							log += " " + targetCharacter.coloredUrlName + " drops " + allWeaponDropped + ".";
//						}

//						//If body part is essential, instant death to the character
//						if (chosenBodyPart.importance == IBodyPart.IMPORTANCE.ESSENTIAL){
//							CheckBodyPart (chosenBodyPart.name, targetCharacter);
//						}
//					}
//				}else if(attackSkill.attackType == ATTACK_TYPE.MAGIC){
//					if(chance < 5){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.BURNING);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BURNING);
//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							log += " A burnt smell emanates from " + targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + "!";
//						}else{
//							string[] verb = new string[]{ "charred", "burning", "roasting" };
//							log += " " + targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + " is " + verb[Utilities.rng.Next(0, verb.Length)] + "!";
//						}
//					}
//				}
//			}
        }


		private string StatusEffectLog(ECS.Character sourceCharacter, ECS.Character targetCharacter, STATUS_EFFECT statusEffect){
			string log = string.Empty;
			if(statusEffect == STATUS_EFFECT.CONFUSED){
				string[] possibleLogs = new string[] {
					targetCharacter.coloredUrlName + "'s mind got corrupted!",
					targetCharacter.coloredUrlName + " started hallucinating!",
					targetCharacter.coloredUrlName + " started curling in pain and screaming 'Get out of my head!'."
				};

				log = possibleLogs[Utilities.rng.Next(0, possibleLogs.Length)];
			}
			return log;
		}
		//Returns a random body part of a character
		private BodyPart GetRandomBodyPart(ECS.Character character){
			List<BodyPart> allBodyParts = character.bodyParts.Where(x => !x.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)).ToList();
			if(allBodyParts.Count > 0){
				return allBodyParts [Utilities.rng.Next (0, allBodyParts.Count)];
			}else{
				return null;
			}
		}
       
		#endregion

		#region Heal Skill
		private void HealSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			HealSkill healSkill = (HealSkill)skill;	
			targetCharacter.AdjustHP (healSkill.healPower);
			if(sourceCharacter == targetCharacter){
				AddCombatLog(sourceCharacter.coloredUrlName + " used " + healSkill.skillName + " and healed himself/herself for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
			}else if(sourceCharacter == targetCharacter){
				AddCombatLog(sourceCharacter.coloredUrlName + " used " + healSkill.skillName + " and healed " + targetCharacter.coloredUrlName + " for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
			}

		}
		#endregion

		#region Flee Skill
		private void FleeSkill(ECS.Character sourceCharacter, ECS.Character targetCharacter){
			//TODO: ECS.Character flees
			RemoveCharacter(targetCharacter);
			fledCharacters.Add (targetCharacter);
			targetCharacter.SetIsDefeated (true);
            CombatManager.Instance.CharacterContinuesAction(targetCharacter);
            AddCombatLog(targetCharacter.coloredUrlName + " chickened out and ran away!", targetCharacter.currentSide);
		}
		#endregion

		#region Obtain Item Skill
		private void ObtainItemSkill(ECS.Character sourceCharacter, ECS.Character targetCharacter){
			//TODO: ECS.Character obtains an item
			AddCombatLog(targetCharacter.coloredUrlName + " obtained an item.", targetCharacter.currentSide);
		}
		#endregion

		#region Move Skill
		private void MoveSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			if(skill.skillName == "MoveLeft"){
				if (targetCharacter.currentRow != 1) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow - 1);
				}
				AddCombatLog(targetCharacter.coloredUrlName + " moved to the left. (" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
			}else if(skill.skillName == "MoveRight"){
				if (targetCharacter.currentRow != 5) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow + 1);
				}
				AddCombatLog(targetCharacter.coloredUrlName + " moved to the right.(" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
			}
		}
		#endregion

		//This will receive the "CharacterDeath" signal when broadcasted, this is a listener
		internal void CharacterDeath(ECS.Character character){
			RemoveCharacter (character);
			//deadCharacters.Add (character);
			character.SetIsDefeated (true);
			AddCombatLog(character.coloredUrlName + " died horribly!", character.currentSide);
		}

		internal void CharacterFainted(ECS.Character character){
			RemoveCharacter (character);
			faintedCharacters.Add (character);
			character.SetIsDefeated (true);
			AddCombatLog(character.coloredUrlName + " fainted!", character.currentSide);
		}

		//Check essential body part quantity, if all are decapitated, instant death
		private void CheckBodyPart(string bodyPart, ECS.Character character){
			for (int i = 0; i < character.bodyParts.Count; i++) {
				BodyPart characterBodyPart = character.bodyParts [i];
				if(characterBodyPart.name == bodyPart && !characterBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
					return;
				}

				for (int j = 0; j < characterBodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = characterBodyPart.secondaryBodyParts [j];
					if(secondaryBodyPart.name == bodyPart && !secondaryBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
						return;
					}
				}
			}
			InstantDeath (character);
		}

		#region Logs
		public void AddCombatLog(string combatLog, SIDES side) {
			resultsLog.Add(combatLog);
			Debug.Log (combatLog);
			if (CombatPrototypeUI.Instance != null) {
				CombatPrototypeUI.Instance.AddCombatLog(combatLog, side);
			}
		}

		public void ClearCombatLogs() {
			resultsLog.Clear();
			if (CombatPrototypeUI.Instance != null) {
				CombatPrototypeUI.Instance.ClearCombatLogs();
			}
		}
		#endregion

		public ECS.Character GetAliveCharacterByID(int id){
			for (int i = 0; i < this.characterSideACopy.Count; i++) {
				if (!this.characterSideACopy[i].isDead && this.characterSideACopy[i].id == id){
					return this.characterSideACopy [i];
				}
			}
			for (int i = 0; i < this.characterSideBCopy.Count; i++) {
				if (!this.characterSideBCopy[i].isDead && this.characterSideBCopy[i].id == id){
					return this.characterSideBCopy [i];
				}
			}
			return null;
		}

        private int GetFinalAttack(int stat, int level, float weaponAttack) {
            return (int) (((weaponAttack + stat) * (1f + (stat / 2f))) * (1f + ((float) level / 100f)));
        }

        //public ICombatInitializer GetOpposingCharacters(ECS.Character character) {
        //    if (attacker is ECS.Character) {
        //        if ((attacker as Character).id == character.id) {
        //            return defender;
        //        }
        //    } else if (attacker is Party) {
        //        if ((attacker as Party).partyMembers.Contains(character)) {
        //            return defender;
        //        }
        //    }

        //    if (defender is ECS.Character) {
        //        if ((defender as Character).id == character.id) {
        //            return attacker;
        //        }
        //    } else if (defender is Party) {
        //        if ((defender as Party).partyMembers.Contains(character)) {
        //            return attacker;
        //        }
        //    }

        //    return null;
        //}
    }
}

