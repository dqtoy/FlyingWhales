using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

	public class CombatPrototype : MonoBehaviour {
		public static CombatPrototype Instance;

        public float updateIntervals = 0.5f;

//		public Dictionary<SIDES, List<ECS.Character>> allCharactersAndSides;
		internal List<ECS.Character> charactersSideA;
		internal List<ECS.Character> charactersSideB;

		void Awake(){
			Instance = this;
		}

		void Start(){
//			this.allCharactersAndSides = new Dictionary<SIDES, List<ECS.Character>> ();
			this.charactersSideA = new List<ECS.Character> ();
			this.charactersSideB = new List<ECS.Character> ();
			Messenger.AddListener<ECS.Character> ("CharacterDeath", CharacterDeath);
		}

        #region ECS.Character Management
        //Add a character to a side
        internal void AddCharacter(SIDES side, ECS.Character character) {
            if (side == SIDES.A) {
                this.charactersSideA.Add(character);
            } else {
                this.charactersSideB.Add(character);
            }
			character.SetSide (side);
            CombatPrototypeUI.Instance.UpdateCharactersList(side);
        }
        //Remove a character from a side
        internal void RemoveCharacter(SIDES side, ECS.Character character) {
            if (side == SIDES.A) {
                this.charactersSideA.Remove(character);
            } else {
                this.charactersSideB.Remove(character);
            }
            CombatPrototypeUI.Instance.UpdateCharactersList(side);
        }
        //Remove character without specifying a side
        internal void RemoveCharacter(ECS.Character character) {
            if (this.charactersSideA.Remove(character)) {
                CombatPrototypeUI.Instance.UpdateCharactersList(SIDES.A);
            } else {
                this.charactersSideB.Remove(character);
                CombatPrototypeUI.Instance.UpdateCharactersList(SIDES.B);
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

        public void StartCombatSimulationCoroutine() {
            StartCoroutine(CombatSimulation());
        }

        //This simulates the whole combat system
        public IEnumerator CombatSimulation(){
            CombatPrototypeUI.Instance.ClearCombatLogs();
			Dictionary<ECS.Character, int> characterActivationWeights = new Dictionary<ECS.Character, int> ();
            bool isInitial = true;
			SetRowNumber (this.charactersSideA, 1);
			SetRowNumber (this.charactersSideB, 5);

            int rounds = 1;
			while(this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0){
                Debug.Log("========== Round " + rounds.ToString() + " ==========");
				ECS.Character characterThatWillAct = GetCharacterThatWillAct (characterActivationWeights, this.charactersSideA, this.charactersSideB, isInitial);
				characterThatWillAct.EnableDisableSkills ();
                Debug.Log(characterThatWillAct.characterClass.className + " " + characterThatWillAct.name + " will act");
                Debug.Log("Available Skills: ");
                for (int i = 0; i < characterThatWillAct.skills.Count; i++) {
                    Skill currSkill = characterThatWillAct.skills[i];
                    if (currSkill.isEnabled) {
                        Debug.Log(currSkill.skillName);
                    }
                }

                Skill skillToUse = GetSkillToUse (characterThatWillAct);
				if(skillToUse != null){
                    Debug.Log(characterThatWillAct.name + " decides to use " + skillToUse.skillName);
					characterThatWillAct.CureStatusEffects ();
					ECS.Character targetCharacter = GetTargetCharacter (characterThatWillAct, skillToUse);
                    Debug.Log(characterThatWillAct.name + " decides to use it on " + targetCharacter.name);
                    DoSkill (skillToUse, characterThatWillAct, targetCharacter);
				}
                if (isInitial) {
                    isInitial = false;
                }
                CombatPrototypeUI.Instance.UpdateCharacterSummary();
                Debug.Log("========== End Round " + rounds.ToString() + " ==========");
                rounds++;
                yield return new WaitForSeconds(updateIntervals);
            }
		}

		//Set row number to a list of characters
		private void SetRowNumber(List<ECS.Character> characters, int rowNumber){
			for (int i = 0; i < characters.Count; i++) {
				characters [i].SetRowNumber(rowNumber);
			}
		}

		//Return a character that will act from a pool of characters based on their act rate
		private ECS.Character GetCharacterThatWillAct(Dictionary<ECS.Character, int> characterActivationWeights, List<ECS.Character> charactersSideA, List<ECS.Character> charactersSideB, bool isInitial){
			characterActivationWeights.Clear();
			if(isInitial){
				for (int i = 0; i < charactersSideA.Count; i++) {
					charactersSideA[i].actRate = charactersSideA [i].agility * 5;
					characterActivationWeights.Add (charactersSideA [i], charactersSideA[i].actRate);
				}
				for (int i = 0; i < charactersSideB.Count; i++) {
					charactersSideB[i].actRate = charactersSideB [i].agility * 5;
					characterActivationWeights.Add (charactersSideB [i], charactersSideB[i].actRate);
				}
			}else{
				for (int i = 0; i < charactersSideA.Count; i++) {
					characterActivationWeights.Add (charactersSideA [i], charactersSideA[i].actRate);
				}
				for (int i = 0; i < charactersSideB.Count; i++) {
					characterActivationWeights.Add (charactersSideB [i], charactersSideB[i].actRate);
				}
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
				if (sourceCharacter.currentSide == SIDES.A) {
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						ECS.Character targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				} else {
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						ECS.Character targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				}
			} else if (skill is HealSkill) {
				if (sourceCharacter.currentSide == SIDES.A) {
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						ECS.Character targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				} else {
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						ECS.Character targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				}
			}else{
				possibleTargets.Add (sourceCharacter);
			}

			return possibleTargets [UnityEngine.Random.Range (0, possibleTargets.Count)];
		}

		//Get Skill that the character will use based on activation weights, target character must be within skill range
		private Skill GetSkillToUse(ECS.Character sourceCharacter){
			Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int> ();
			Dictionary<object, int> categoryActivationWeights = new Dictionary<object, int> ();

			//First step: pick from general skills or body part skill or weapon skill
			if (sourceCharacter.HasActivatableBodyPartSkill ()) {
				categoryActivationWeights.Add ("bodypart", 10);
			}
			if(sourceCharacter.HasActivatableWeaponSkill()){
				categoryActivationWeights.Add("weapon", 100);
			}
			for (int i = 0; i < sourceCharacter.skills.Count; i++) {
				Skill skill = sourceCharacter.skills [i];
				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.GENERAL){
					int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
					if(skill is MoveSkill && HasTargetInRangeForSkill(SKILL_TYPE.ATTACK, sourceCharacter)){
						activationWeight /= 2;
					}
					categoryActivationWeights.Add (skill, activationWeight);
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
			if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.CURRENT_HEALTH){
				activationWeight *= ((int)(((float)sourceCharacter.currentHP / (float)sourceCharacter.maxHP) * 100f));
			}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.MISSING_HEALTH){
				int missingHealth = sourceCharacter.maxHP - sourceCharacter.currentHP;
				int weight = (int)(((float)missingHealth / (float)sourceCharacter.maxHP) * 100f);
				activationWeight *=  (weight > 0f ? weight : 1);
			}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.ALLY_MISSING_HEALTH){
				int highestMissingHealth = 0;
				ECS.Character chosenCharacter = null;
				if(sourceCharacter.currentSide == SIDES.A){
					for (int j = 0; j < charactersSideA.Count; j++) {
						ECS.Character character = charactersSideA [j];
						int missingHealth = character.maxHP - character.currentHP;
						if(chosenCharacter == null){
							highestMissingHealth = missingHealth;
							chosenCharacter = character;
						}else{
							if(missingHealth > highestMissingHealth){
								highestMissingHealth = missingHealth;
								chosenCharacter = character;
							}
						}
					}
				}else{
					for (int j = 0; j < charactersSideB.Count; j++) {
						ECS.Character character = charactersSideB [j];
						int missingHealth = character.maxHP - character.currentHP;
						if(chosenCharacter == null){
							highestMissingHealth = missingHealth;
							chosenCharacter = character;
						}else{
							if(missingHealth > highestMissingHealth){
								highestMissingHealth = missingHealth;
								chosenCharacter = character;
							}
						}
					}
				}
				if(chosenCharacter != null){
					int weight = (int)((((float)highestMissingHealth / (float)chosenCharacter.maxHP) * 100f) * 2f);
					activationWeight *= (weight > 0f ? weight : 1);
				}
			}
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
						if(sourceCharacter.currentSide == SIDES.A){
							for (int j = 0; j < this.charactersSideB.Count; j++) {
								ECS.Character targetCharacter = this.charactersSideB [j];
								int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
								if(skill.range >= rowDistance){
									return true;
								}
							}
						}else{
							for (int j = 0; j < this.charactersSideA.Count; j++) {
								ECS.Character targetCharacter = this.charactersSideA [j];
								int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
								if(skill.range >= rowDistance){
									return true;
								}
							}
						}
						return false;
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
			float chance = UnityEngine.Random.Range (0f, 99f);
			if(chance < skill.accuracy){
				//Successful
				SuccessfulSkill(skill, sourceCharacter, targetCharacter);
			}else{
				//Fail
				FailedSkill(skill, sourceCharacter, targetCharacter);
			}
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
				CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " tried to flee but got tripped over and fell down!", sourceCharacter.currentSide);
//				CounterAttack (targetCharacter);
			}else if(skill is AttackSkill){
				CombatPrototypeUI.Instance.AddCombatLog (sourceCharacter.name + " tried to " + skill.skillName.ToLower() + " " + targetCharacter.name + " but missed!", sourceCharacter.currentSide);
			}else if(skill is HealSkill){
				if(sourceCharacter == targetCharacter){
					CombatPrototypeUI.Instance.AddCombatLog (sourceCharacter.name + " tried to use " + skill.skillName.ToLower() + " to heal himself/herself but it is already expired!", sourceCharacter.currentSide);
				}else{
					CombatPrototypeUI.Instance.AddCombatLog (sourceCharacter.name + " tried to use " + skill.skillName.ToLower() + " to heal " + targetCharacter.name + " but it is already expired!", sourceCharacter.currentSide);
				}
			}
		}

		//Get DEFEND_TYPE for the attack skill, if DEFEND_TYPE is NONE, then target character has not defend successfully, therefore, the target character will be damaged
		private DEFEND_TYPE CanTargetCharacterDefend(ECS.Character targetCharacter){
			int dodgeChance = UnityEngine.Random.Range (0, 100);
			if(dodgeChance < targetCharacter.dodgeRate){
				return DEFEND_TYPE.DODGE;
			}else{
				int parryChance = UnityEngine.Random.Range (0, 100);
				if(parryChance < targetCharacter.parryRate){
					return DEFEND_TYPE.PARRY;
				}else{
					int blockChance = UnityEngine.Random.Range (0, 100);
					if(blockChance < targetCharacter.blockRate){
						return DEFEND_TYPE.BLOCK;
					}else{
						return DEFEND_TYPE.NONE;
					}
				}
			}
		}

		private void CounterAttack(ECS.Character character){
			//TODO: Counter attack
			CombatPrototypeUI.Instance.AddCombatLog (character.name + " counterattacked!", character.currentSide);
		}

		private void InstantDeath(ECS.Character character){
			character.Death();
		}

		#region Attack Skill
		private void AttackSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			AttackSkill attackSkill = (AttackSkill)skill;
			DEFEND_TYPE defendType = CanTargetCharacterDefend (targetCharacter);
			if(defendType == DEFEND_TYPE.NONE){
				//Successfully hits the target character
				HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter, attackSkill.weapon);
			}else{
				//Target character has defend successfully and will roll for counter attack
				if(defendType == DEFEND_TYPE.DODGE){
					CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " dodged " + sourceCharacter.name + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
				} else if(defendType == DEFEND_TYPE.BLOCK){
					CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " blocked " + sourceCharacter.name + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
				} else if(defendType == DEFEND_TYPE.PARRY){
					CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " parried " + sourceCharacter.name + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
				}
				CounterAttack(targetCharacter);
			}
		}
			
		//Hits the target with an attack skill
		private void HitTargetCharacter(AttackSkill attackSkill, ECS.Character sourceCharacter, ECS.Character targetCharacter, Weapon weapon = null){
			//Total Damage = [Weapon Power + (Int or Str)] - [Base Damage Mitigation] - [Bonus Attack Type Mitigation] + [Bonus Attack Type Weakness]
			string log = string.Empty;
			float weaponPower = 0f;
			BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
			if (chosenBodyPart == null) {
				Debug.LogError ("NO MORE BODY PARTS!");
				return;
			}
			Armor armor = chosenBodyPart.GetArmor ();
			log += sourceCharacter.name + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.name + " in the " + chosenBodyPart.name.ToLower();

            if(weapon != null) {
				weaponPower = weapon.weaponPower;
				if(Utilities.GetMaterialCategory(weapon.material) == MATERIAL_CATEGORY.WOOD && (weapon.weaponType == WEAPON_TYPE.BOW || weapon.weaponType == WEAPON_TYPE.STAFF)){
					weaponPower *= 2f;
				}
				//reduce weapon durability by durability cost of skill
				weapon.AdjustDurability(-attackSkill.durabilityCost);
				log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " " + weapon.itemName + ".";
			}else{
				log += ".";
			}
			int damage = (int)(weaponPower + (attackSkill.attackType == ATTACK_TYPE.MAGIC ? sourceCharacter.intelligence : sourceCharacter.strength));

			if(armor != null){
				if(attackSkill.attackType != ATTACK_TYPE.PIERCE){
					int damageNullChance = UnityEngine.Random.Range (0, 100);
					if(damageNullChance < armor.damageNullificationChance){
						log += " The attack was fully absorbed by the " + armor.itemName + ".";
						return;
					}
					damage -= (int)((float)damage * (armor.baseDamageMitigation / 100f));
				}else{
					damage -= (int)((float)damage * ((armor.baseDamageMitigation / 2f) / 100f));
				}
				if(armor.ineffectiveAttackTypes.Contains(attackSkill.attackType)){
					damage -= (int)((float)damage * 0.2f);
				}
				if(armor.effectiveAttackTypes.Contains(attackSkill.attackType)){
					damage += (int)((float)damage * 0.2f);
				}
				armor.AdjustDurability (-attackSkill.durabilityDamage);
			}
			log += "(" + damage.ToString () + ")";

			DealDamageToBodyPart (attackSkill, targetCharacter, sourceCharacter, chosenBodyPart, ref log);

			CombatPrototypeUI.Instance.AddCombatLog (log, sourceCharacter.currentSide);

			targetCharacter.AdjustHP (-damage);

            //Get all armor pieces on body part that still has hit points
//			List<Item> armorOnBodyPart = damagedBodyPart.GetAttachedItemsOfType(ITEM_TYPE.ARMOR).OrderByDescending(x => ((Armor)x).currHitPoints).ToList();
//            if (armorOnBodyPart.Count > 0) {
//				if(attackSkill.attackType != ATTACK_TYPE.PIERCE){
//					for (int i = 0; i < armorOnBodyPart.Count; i++) {
//						Armor currArmor = (Armor)armorOnBodyPart[i];
//						if(currArmor.currHitPoints > 0) {
//							//Deal damage to armor
//							hasDamagedArmor = true;
//							int damageToArmor = 0;
//							if(damage > currArmor.currHitPoints){
//								damageToArmor = currArmor.currHitPoints;
//							}else{
//								damageToArmor = damage;
//							}
//							damage -= damageToArmor;
//
//							currArmor.AdjustHitPoints (-damageToArmor);
//							CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and damages " + targetCharacter.name
//								+ "'s " + currArmor.itemName + " for " + damageToArmor.ToString() + " and damages " + targetCharacter.name + " for " + damage.ToString() + ".");
//
//							targetCharacter.AdjustHP(-damage);
//
//							if(damage < 0 ){
//								damage = 0;
//							}
//						}
//					}
//				}
//				if (damage > 0 && !hasDamagedArmor) {
//					//Deal damage to hp
//					CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and damages " + targetCharacter.name
//						+ " for " + damage.ToString() + ".");
//				}
//
//				int damageToDurability = attackSkill.durabilityDamage; //[SkillDurabilityDamage  +  WeaponDurabilityDamage]
//				if (weapon != null) {
//					damageToDurability += weapon.durabilityDamage;
//				}
//				for (int i = 0; i < armorOnBodyPart.Count; i++) {
//					Armor currArmor = (Armor)armorOnBodyPart[i];
//					if(currArmor.durability > 0) {
//						//Reduce Armor Durability
//						currArmor.AdjustDurability(-damageToDurability);
//						if(currArmor.durability <= 0) {
//							CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " destroys " + targetCharacter.name + "'s " + currArmor.itemName);
//						}
//					}
//				}
//
//                
//                    
//            } else {
//                //Deal damage to hp
//                CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and damages " + targetCharacter.name
//					+ " for " + damage.ToString() + ".");
//                    targetCharacter.AdjustHP(-damage);
//            }
        }

		//This will select, deal damage, and apply status effect to a body part if possible 
		private void DealDamageToBodyPart(AttackSkill attackSkill, ECS.Character targetCharacter, ECS.Character sourceCharacter, BodyPart chosenBodyPart, ref string log){
			int chance = UnityEngine.Random.Range (0, 100);

			if(attackSkill.statusEffectRates != null && attackSkill.statusEffectRates.Count > 0){
				for (int i = 0; i < attackSkill.statusEffectRates.Count; i++) {
					int value = attackSkill.statusEffectRates[i].ratePercentage;
					if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.INJURED){
						if(attackSkill.attackType == ATTACK_TYPE.CRUSH){
							value += 7;
//							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and crushes " + targetCharacter.name
//							+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + ", injuring it.";
						}else{
//							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and injures " + targetCharacter.name
//							+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + ".";
						}

					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BLEEDING){
						if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
							value += 10;
//							log = sourceCharacter.name + " used " + attackSkill.skillName + " and pierces " + targetCharacter.name + 
//								"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", causing it to bleed.";
						}else{
//							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and causes " + targetCharacter.name
//								+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + " to bleed.";
						}
					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.DECAPITATED){
						if(attackSkill.attackType == ATTACK_TYPE.SLASH){
							value += 5;
//							log = sourceCharacter.name + " used " + attackSkill.skillName + " and slashes " + targetCharacter.name + 
//								"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", decapitating it.";
						}else{
//							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and decapitates " + targetCharacter.name
//								+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + ".";
						}
					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BURNING){
						if(attackSkill.attackType == ATTACK_TYPE.MAGIC){
							value += 5;
						}
//						log = sourceCharacter.name + " used " + attackSkill.skillName + " and burns " + targetCharacter.name + 
//							"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ".";
					}

					if(chance < value){
						chosenBodyPart.AddStatusEffect(attackSkill.statusEffectRates[i].statusEffect);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (attackSkill.statusEffectRates[i].statusEffect);
//						CombatPrototypeUI.Instance.AddCombatLog (log);
					}
				}
			}else{
				if (attackSkill.attackType == ATTACK_TYPE.CRUSH){
					if(chance < 7){
						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.INJURED);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.INJURED);

						int logChance = UnityEngine.Random.Range (0, 2);
						if(logChance == 0){
							string[] predicate = new string[]{ "battered", "crippled", "mangled", "brokened" };
							log += " " + targetCharacter.name + "'s " + chosenBodyPart.name.ToLower() + " is " + predicate[UnityEngine.Random.Range(0, predicate.Length)] + "!";
						}else{
							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
							if(secondaryBodPart != null){
								log += " " + targetCharacter.name + "'s " + secondaryBodPart.name.ToLower() + " makes a crunching noise!";
							}
						}

					}
				}else if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
					if(chance < 10){
						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.BLEEDING);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BLEEDING);

						int logChance = UnityEngine.Random.Range (0, 2);
						if(logChance == 0){
							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
							if(secondaryBodPart != null){
								string[] adjective = new string[]{ "deep", "light", "painful", "fresh", "deadly" };
								string[] noun = new string[]{ "gash", "wound", "lesion", "tear" };

								log += " A " + adjective[UnityEngine.Random.Range(0, adjective.Length)] + " " + noun[UnityEngine.Random.Range(0, noun.Length)] + " forms near " + targetCharacter.name + "'s " + secondaryBodPart.name.ToLower() + ".";
							}
						}else{
							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
							if(secondaryBodPart != null){
								log += " Blood erupts from " + targetCharacter.name + "'s " + secondaryBodPart.name.ToLower() + "!";
							}
						}
					}
				}else if(attackSkill.attackType == ATTACK_TYPE.SLASH){
					if(chance < 5){
						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.DECAPITATED);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.DECAPITATED);

						string[] verb = new string[]{ "severed", "decapitated", "sliced off", "lopped off" };
						log += targetCharacter.name + "'s " + chosenBodyPart.name.ToLower() + " has been " + verb[UnityEngine.Random.Range(0, verb.Length)] + " by the attack!";

						int logChance = UnityEngine.Random.Range (0, 2);
						if(logChance == 0){
							log += " It drops to the floor lifelessly.";
						}else{
							log += " It flew away!";
						}

						string allWeaponDropped = string.Empty;
						for (int i = 0; i < targetCharacter.equippedItems.Count; i++) {
							Item item = targetCharacter.equippedItems [i];
							if(item is Weapon){
								Weapon weapon = (Weapon)item;
								for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
									if(weapon.bodyPartsAttached[j].statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
										if(allWeaponDropped != string.Empty){
											allWeaponDropped += ", ";
										}
										allWeaponDropped += item.itemName;
										targetCharacter.ThrowItem (item);
										break;
									}
								}
							}
						}
						if(allWeaponDropped != string.Empty){
							log += " " + targetCharacter.name + " drops " + allWeaponDropped + ".";
						}

						//If body part is essential, instant death to the character
						if (chosenBodyPart.importance == IBodyPart.IMPORTANCE.ESSENTIAL){
							CheckBodyPart (chosenBodyPart.bodyPart, targetCharacter);
						}
					}
				}else if(attackSkill.attackType == ATTACK_TYPE.MAGIC){
					if(chance < 5){
						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.BURNING);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BURNING);
						int logChance = UnityEngine.Random.Range (0, 2);
						if(logChance == 0){
							log += " A burnt smell emanates from " + targetCharacter.name + "'s " + chosenBodyPart.name.ToLower() + "!";
						}else{
							string[] verb = new string[]{ "charred", "burning", "roasting" };
							log += " " + targetCharacter.name + "'s " + chosenBodyPart.name.ToLower() + " is " + verb[UnityEngine.Random.Range(0, verb.Length)] + "!";
						}
					}
				}
			}
        }


		//Returns a random body part of a character
		private BodyPart GetRandomBodyPart(ECS.Character character){
//			List<IBodyPart> allBodyParts = new List<IBodyPart> (character.bodyParts);
//			for (int i = 0; i < character.bodyParts.Count; i++) {
//				allBodyParts.AddRange (character.bodyParts [i].secondaryBodyParts);
//			}

//			return allBodyParts [UnityEngine.Random.Range (0, allBodyParts.Count)];
			List<BodyPart> allBodyParts = character.bodyParts.Where(x => !x.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)).ToList();
			if(allBodyParts.Count > 0){
				return allBodyParts [UnityEngine.Random.Range (0, allBodyParts.Count)];
			}else{
				return null;
			}
		}
        /*
         * Get Weapon that meets the requirements of a given skill,
         * if the skill does not require a weapon, this will just return null.
         * */
//        private Weapon GetWeaponForSkill(Skill skill, ECS.Character sourceCharacter) {
//            if (skill.RequiresItem()) {
//                List<Weapon> weapons = sourceCharacter.GetAllAttachedWeapons();
//                for (int i = 0; i < weapons.Count; i++) {
//                    Weapon currWeapon = weapons[i];
//                    bool meetsRequirements = true;
//                    for (int j = 0; j < skill.skillRequirements.Length; j++) {
//                        SkillRequirement currRequirement = skill.skillRequirements[j];
//                        if(currRequirement.equipmentType == EQUIPMENT_TYPE.NONE) {
//                            //skip requirements that don't require an item
//                            continue;
//                        }
//                        if(currRequirement.equipmentType != (EQUIPMENT_TYPE)currWeapon.weaponType || !currWeapon.attributes.Contains(currRequirement.attributeRequired)) {
//                            meetsRequirements = false;
//                            break;
//                        }
//                    }
//                    if (meetsRequirements) {
//                        return currWeapon;
//                    }
//                }
//            }
//            return null;
//        }
		#endregion

		#region Heal Skill
		private void HealSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			HealSkill healSkill = (HealSkill)skill;	
			targetCharacter.AdjustHP (healSkill.healPower);
			if(sourceCharacter == targetCharacter){
				CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + healSkill.skillName + " and healed himself/herself for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
			}else if(sourceCharacter == targetCharacter){
				CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + healSkill.skillName + " and healed " + targetCharacter.name + " for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
			}

		}
		#endregion

		#region Flee Skill
		private void FleeSkill(ECS.Character sourceCharacter, ECS.Character targetCharacter){
			//TODO: ECS.Character flees
			RemoveCharacter(targetCharacter);
			CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " chickened out and ran away!", targetCharacter.currentSide);
		}
		#endregion

		#region Obtain Item Skill
		private void ObtainItemSkill(ECS.Character sourceCharacter, ECS.Character targetCharacter){
			//TODO: ECS.Character obtains an item
			CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " obtained an item.", targetCharacter.currentSide);
		}
		#endregion

		#region Move Skill
		private void MoveSkill(Skill skill, ECS.Character sourceCharacter, ECS.Character targetCharacter){
			if(skill.skillName == "MoveLeft"){
				if (targetCharacter.currentRow != 1) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow - 1);
				}
				CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " moved to the left. (" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
			}else if(skill.skillName == "MoveRight"){
				if (targetCharacter.currentRow != 5) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow + 1);
				}
				CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " moved to the right.(" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
			}

//			if(sourceCharacter.currentRow == 1){
//				//Automatically moves to the right since 1 is the last row on the left
//				sourceCharacter.SetRowNumber (2);
//			}else if(sourceCharacter.currentRow == 5){
//				//Automatically moves to the left since 5 is the last row on the right
//				sourceCharacter.SetRowNumber (4);
//			}else{
//				//TODO: Is it totally random, or there will determining factors if movement is to the left or to the right
//				int chance = UnityEngine.Random.Range (0, 2);
//				if(chance == 0){
//					//Move left
//					sourceCharacter.SetRowNumber(sourceCharacter.currentRow - 1);
//				}else{
//					//Move right
//					sourceCharacter.SetRowNumber(sourceCharacter.currentRow + 1);
//				}
//			}
		}
		#endregion

		//This will receive the "CharacterDeath" signal when broadcasted, this is a listener
		private void CharacterDeath(ECS.Character character){
			RemoveCharacter (character);
			CombatPrototypeUI.Instance.AddCombatLog(character.name + " died horribly!", character.currentSide);
		}

		//Check essential body part quantity, if all are decapitated, instant death
		private void CheckBodyPart(BODY_PART bodyPart, ECS.Character character){
			for (int i = 0; i < character.bodyParts.Count; i++) {
				BodyPart characterBodyPart = character.bodyParts [i];
				if(characterBodyPart.bodyPart == bodyPart && !characterBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
					return;
				}

				for (int j = 0; j < characterBodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = characterBodyPart.secondaryBodyParts [j];
					if(secondaryBodyPart.bodyPart == bodyPart && !secondaryBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
						return;
					}
				}
			}
			InstantDeath (character);
		}

        #region Items
//        private void ResetAllArmorHitPoints() {
//            List<ECS.Character> allCharacters = new List<ECS.Character>(charactersSideA);
//            allCharacters.AddRange(charactersSideB);
//            for (int i = 0; i < allCharacters.Count; i++) {
//                ECS.Character currCharacter = allCharacters[i];
//                List<Armor> armorOfCharacter = currCharacter.GetAllAttachedArmor();
//                for (int j = 0; j < armorOfCharacter.Count; j++) {
//                    Armor currArmor = armorOfCharacter[j];
//                    currArmor.ResetHitPoints();
//                }
//            }
//        }
        #endregion
    }
}

