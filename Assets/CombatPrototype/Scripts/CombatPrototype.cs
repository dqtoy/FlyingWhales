using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

	public class CombatPrototype : MonoBehaviour {
		public CombatPrototype Instance;

//		public Dictionary<SIDES, List<Character>> allCharactersAndSides;
		List<Character> charactersSideA;
		List<Character> charactersSideB;

		void Awake(){
			Instance = this;
		}

		void Start(){
//			this.allCharactersAndSides = new Dictionary<SIDES, List<Character>> ();
			this.charactersSideA = new List<Character> ();
			this.charactersSideB = new List<Character> ();
		}

		//Add a character to a side
		internal void AddCharacter(SIDES side, Character character){
//			if(this.allCharactersAndSides.ContainsKey(side)){
//				this.allCharactersAndSides [side].Add (character);
//			}else{
//				this.allCharactersAndSides.Add (side, new List<Character>(){character});
//			}

			if(side == SIDES.A){
				this.charactersSideA.Add (character);
			}else{
				this.charactersSideB.Add (character);
			}
		}

		//Remove a character from a side
		internal void RemoveCharacter(SIDES side, Character character){
//			if(this.allCharactersAndSides.ContainsKey(side)){
//				this.allCharactersAndSides [side].Remove (character);
//			}

			if(side == SIDES.A){
				this.charactersSideA.Remove (character);
			}else{
				this.charactersSideB.Remove (character);
			}
		}

		//Remove character without specifying sides
		internal void RemoveCharacter(Character character){
			this.charactersSideA.Remove (character);
			this.charactersSideB.Remove (character);
		}

		//This simulates the whole combat system
		internal void CombatSimulation(){
//			List<Character> charactersSideA = this.allCharactersAndSides [SIDES.A];
//			List<Character> charactersSideB = this.allCharactersAndSides [SIDES.B];

			bool isInitial = true;
			bool isOneSideDefeated = false;
			SetRowNumber (this.charactersSideA, 1);
			SetRowNumber (this.charactersSideB, 5);

			while(this.charactersSideA.Count > 0 && this.charactersSideA.Count > 0){
				Character characterThatWillAct = GetCharacterThatWillAct (this.charactersSideA, this.charactersSideB, isInitial);
				characterThatWillAct.EnableDisableSkills ();

				//TODO: Which is first get target character or get skill to use, example confusion is Heal Skill
				Character targetCharacter = GetTargetCharacter (characterThatWillAct);

				Skill skillToUse = GetSkillToUse (characterThatWillAct, targetCharacter);
				if(skillToUse != null){
					DoSkill (skillToUse, characterThatWillAct, targetCharacter);
				}
			}


		}

		//Set row number to a list of characters
		private void SetRowNumber(List<Character> characters, int rowNumber){
			for (int i = 0; i < characters.Count; i++) {
				characters [i].SetRowNumber(rowNumber);
			}
		}

		//Return a character that will act from a pool of characters based on their act rate
		private Character GetCharacterThatWillAct(List<Character> charactersSideA, List<Character> charactersSideB, bool isInitial){
			Dictionary<Character, int> characterActivationWeights = new Dictionary<Character, int> ();
			int modifier = 1;
			if(isInitial){
				modifier = 10;
			}

			for (int i = 0; i < charactersSideA.Count; i++) {
				int actRate = charactersSideA [i].characterClass.actRate * modifier;
				characterActivationWeights.Add (charactersSideA [i], actRate);
			}
			for (int i = 0; i < charactersSideB.Count; i++) {
				int actRate = charactersSideB [i].characterClass.actRate * modifier;
				characterActivationWeights.Add (charactersSideB [i], actRate);
			}

			Character chosenCharacter = Utilities.PickRandomElementWithWeights<Character> (characterActivationWeights);
			foreach (Character character in characterActivationWeights.Keys) {
				character.characterClass.actRate += character.characterClass.actRate;
			}
			chosenCharacter.characterClass.actRate = 0;
			return chosenCharacter;
		}

		//Get a random character from the opposite side to be the target
		private Character GetTargetCharacter(Character sourceCharacter){
//			if(this.allCharactersAndSides[SIDES.A].Contains(sourceCharacter)){
//				return this.allCharactersAndSides [SIDES.B] [UnityEngine.Random.Range (0, this.allCharactersAndSides [SIDES.B].Count)];
//			}
//			return this.allCharactersAndSides [SIDES.A] [UnityEngine.Random.Range (0, this.allCharactersAndSides [SIDES.A].Count)];

			if(this.charactersSideA.Contains(sourceCharacter)){
				return this.charactersSideB [UnityEngine.Random.Range (0, this.charactersSideB.Count)];
			}
			return this.charactersSideA [UnityEngine.Random.Range (0, this.charactersSideA.Count)];
		}

		//Get Skill that the character will use based on activation weights, target character must be within skill range
		private Skill GetSkillToUse(Character sourceCharacter, Character targetCharacter){
			Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int> ();
			int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
			for (int i = 0; i < sourceCharacter.characterClass.skills.Count; i++) {
				Skill skill = sourceCharacter.characterClass.skills [i];
				if(skill.isEnabled && skill.range >= rowDistance){
					skillActivationWeights.Add (skill, skill.activationWeight);
				}
			}

			if(skillActivationWeights.Count > 0){
				Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill> (skillActivationWeights);
				return chosenSkill;
			}
			return null;
		}

		//Returns the row distance/difference of two characters
		private int GetRowDistanceBetweenTwoCharacters(Character sourceCharacter, Character targetCharacter){
			int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
			if(distance < 0){
				distance *= -1;
			}
			return distance;
		}
		//Character will do the skill specified, but its success will be determined by the skill's accuracy
		private void DoSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
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
		private void SuccessfulSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			if (skill is AttackSkill) {
				AttackSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is HealSkill) {
				HealSkill (skill, sourceCharacter);
			} else if (skill is FleeSkill) {
				FleeSkill (sourceCharacter);
			} else if (skill is ObtainSkill) {
				ObtainItemSkill (sourceCharacter);
			} else if (skill is MoveSkill) {
				MoveSkill (sourceCharacter);
			}
		}

		//Skill is not accurate and therefore has failed to execute
		private void FailedSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			//TODO: What happens when a skill has failed?
			if(skill is FleeSkill){
				CounterAttack ();
			}
		}

		//Get DEFEND_TYPE for the attack skill, if DEFEND_TYPE is NONE, then target character has not defend successfully, therefore, the target character will be damaged
		private DEFEND_TYPE CanTargetCharacterDefend(Character targetCharacter){
			int dodgeChance = UnityEngine.Random.Range (0, 100);
			if(dodgeChance < targetCharacter.characterClass.dodgeRate){
				return DEFEND_TYPE.DODGE;
			}else{
				int parryChance = UnityEngine.Random.Range (0, 100);
				if(parryChance < targetCharacter.characterClass.parryRate){
					return DEFEND_TYPE.PARRY;
				}else{
					int blockChance = UnityEngine.Random.Range (0, 100);
					if(blockChance < targetCharacter.characterClass.blockRate){
						return DEFEND_TYPE.BLOCK;
					}else{
						return DEFEND_TYPE.NONE;
					}
				}
			}
		}

		private void CounterAttack(){
			//TODO: Counter attack
		}

		private void InstantDeath(Character character){
			//TODO: Instant death
		}

		#region Attack Skill
		private void AttackSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			AttackSkill attackSkill = (AttackSkill)skill;
			if(CanTargetCharacterDefend(targetCharacter) == DEFEND_TYPE.NONE){
				//Successfully hits the target character
				HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter);
			}else{
				//Target character has defend successfully and will roll for counter attack
				CounterAttack();
			}
		}
			
		//Hits the target with an attack skill
		private void HitTargetCharacter(AttackSkill attackSkill, Character sourceCharacter, Character targetCharacter){
			//TODO: damage computation must be power + attributeModifier * armor damage mitigation, add armor ddamage mitigation variable
			int damage = attackSkill.attackPower;

			targetCharacter.AdjustHP (-damage);

			DealDamageToBodyPart (attackSkill, targetCharacter);
		}

		//This will select, deal damage, and apply status effect to a body part if possible 
		private void DealDamageToBodyPart(AttackSkill attackSkill, Character targetCharacter){
			int chance = UnityEngine.Random.Range (0, 100);

			if(attackSkill.attackType == ATTACK_TYPE.CRUSH){
				if(chance < 7){
					BodyPart chosenBodyPart = GetRandomBodyPart (targetCharacter);
					chosenBodyPart.status = IBodyPart.STATUS.INJURED;
				}
			}else if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
				if(chance < 10){
					BodyPart chosenBodyPart = GetRandomBodyPart (targetCharacter);
					chosenBodyPart.status = IBodyPart.STATUS.BLEEDING;
				}
			}else if(attackSkill.attackType == ATTACK_TYPE.SLASH){
				if(chance < 5){
					BodyPart chosenBodyPart = GetRandomBodyPart (targetCharacter);
					chosenBodyPart.status = IBodyPart.STATUS.DECAPITATED;
					//If body part is essential, instant death to the character
					//TODO: Checking of number of essential of the same type of body part before doing instant death
				}
			}else if(attackSkill.attackType == ATTACK_TYPE.BURN){
				if(chance < 5){
					BodyPart chosenBodyPart = GetRandomBodyPart (targetCharacter);
					chosenBodyPart.status = IBodyPart.STATUS.BURNING;
				}
			}
		}


		//Returns a random body part of a character
		private BodyPart GetRandomBodyPart(Character character){
//			List<IBodyPart> allBodyParts = new List<IBodyPart> (character.bodyParts);
//			for (int i = 0; i < character.bodyParts.Count; i++) {
//				allBodyParts.AddRange (character.bodyParts [i].secondaryBodyParts);
//			}

//			return allBodyParts [UnityEngine.Random.Range (0, allBodyParts.Count)];

			return character.bodyParts [UnityEngine.Random.Range (0, character.bodyParts.Count)];
		}
		#endregion

		#region Heal Skill
		private void HealSkill(Skill skill, Character sourceCharacter){
			HealSkill healSkill = (HealSkill)skill;	
			sourceCharacter.AdjustHP (healSkill.healPower);
		}
		#endregion

		#region Flee Skill
		private void FleeSkill(Character sourceCharacter){
			//TODO: Character flees
			RemoveCharacter(sourceCharacter);
		}
		#endregion

		#region Obtain Item Skill
		private void ObtainItemSkill(Character sourceCharacter){
			//TODO: Character obtains an item
		}
		#endregion

		#region Move Skill
		private void MoveSkill(Character sourceCharacter){
			if(sourceCharacter.currentRow == 1){
				//Automatically moves to the right since 1 is the last row on the left
				sourceCharacter.SetRowNumber (2);
			}else if(sourceCharacter.currentRow == 5){
				//Automatically moves to the left since 5 is the last row on the right
				sourceCharacter.SetRowNumber (4);
			}else{
				//TODO: Is it totally random, or there will determining factors if movement is to the left or to the right
				int chance = UnityEngine.Random.Range (0, 2);
				if(chance == 0){
					//Move left
					sourceCharacter.SetRowNumber(sourceCharacter.currentRow - 1);
				}else{
					//Move right
					sourceCharacter.SetRowNumber(sourceCharacter.currentRow + 1);
				}
			}
		}
		#endregion
	}
}

