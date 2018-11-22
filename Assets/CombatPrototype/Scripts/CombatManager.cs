using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ECS {
    public class CombatManager : MonoBehaviour {
        public static CombatManager Instance = null;

        [NonSerialized] public float updateIntervals = 0f;
        public int numOfCombatActionPerDay;

        public Combat combat;

        public CharacterSetup[] baseCharacters;
		public Color[] characterColors;
		public Dictionary<WEAPON_TYPE, List<Skill>> weaponTypeSkills;

		private List<Color> unusedColors;
		private List<Color> usedColors;

        private void Awake() {
            Instance = this;
        }
		internal void Initialize(){
            //ConstructBaseCharacters();
            weaponTypeSkills = new Dictionary<WEAPON_TYPE, List<Skill>>();
            unusedColors = new List<Color>();
            usedColors = new List<Color>();
            //ConstructCharacterColors ();
            //			ConstructAttributeSkills ();
            //NewCombat();
            //_combatRooms = new Dictionary<Character, CombatRoom>();
            //Messenger.AddListener<Character, Character>(Signals.COLLIDED_WITH_CHARACTER, CheckForCombat);
        }
        private void ConstructBaseCharacters() {
            string path = Utilities.dataPath + "CharacterSetups/";
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
        //internal ECS.Character CreateNewCharacter(CharacterSetup baseCharacter) {
        //    return new ECS.Character(baseCharacter, Utilities.GetRandomGender());
        //}

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

        internal CharacterSetup GetBaseCharacterSetup(string className) {
            for (int i = 0; i < this.baseCharacters.Length; i++) {
                CharacterSetup currBase = this.baseCharacters[i];
                if (currBase.characterClassName.ToLower() == className.ToLower()) {
                    return currBase;
                }
            }
            return null;
        }
		//public void CombatResults(Combat combat){
		//	//for (int i = 0; i < combat.deadCharacters.Count; i++) {
  // //             Character currDeadCharacter = combat.deadCharacters[i];
  // //             currDeadCharacter.Death ();
		//	//}
  //          while(combat.charactersSideA.Count > 0) { //characters that are still alive after combat
  //              ICharacter icharacter = combat.charactersSideA[0];
  //              if (icharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
  //                  Character character = icharacter as Character;
  //                  //CharacterParty characterParty = character.currentParty as CharacterParty;
  //                  //if (!character.IsInOwnParty()) { //characters that do not own the party they are in
  //                  //    if (characterParty.IsOwnerDead()) { //if the owner of the party is dead, disband the party
  //                  //        characterParty.DisbandPartyKeepOwner();
  //                  //    }
  //                  //}
  //                  if (icharacter.ownParty.iactionData.isHalted) {
  //                      icharacter.ownParty.iactionData.SetIsHalted(false);
  //                  }
  //              }
  //              //icharacter.ResetToFullHP();
  //              icharacter.ownParty.currentCombat = null;
  //              //icharacter.currentParty.currentCombat = null;
  //              icharacter.ResetToFullSP();
  //              combat.RemoveCharacter(SIDES.A, icharacter);
  //          }
  //          while (combat.charactersSideB.Count > 0) { //characters that are still alive after combat
  //              ICharacter icharacter = combat.charactersSideB[0];
  //              if (icharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
  //                  Character character = icharacter as Character;
  //                  //CharacterParty characterParty = character.currentParty as CharacterParty;
  //                  //if (!character.IsInOwnParty()) { //characters that do not own the party they are in
  //                  //    if (characterParty.IsOwnerDead()) { //if the owner of the party is dead, disband the party
  //                  //        characterParty.DisbandPartyKeepOwner();
  //                  //    }
  //                  //}
  //                  if (icharacter.ownParty.iactionData.isHalted) {
  //                      icharacter.ownParty.iactionData.SetIsHalted(false);
  //                  }
  //              }
  //              //icharacter.ResetToFullHP();
  //              icharacter.ownParty.currentCombat = null;
  //              //icharacter.currentParty.currentCombat = null;
  //              icharacter.ResetToFullSP();
  //              combat.RemoveCharacter(SIDES.B, icharacter);
  //          }
  //          for (int i = 0; i < combat.afterCombatActions.Count; i++) {
  //              combat.afterCombatActions[i]();
  //          }
            
  //      }

        #region Chance Combat
        public void GetCombatChanceOfTwoLists(List<ICharacter> allies, List<ICharacter> enemies, out float allyChance, out float enemyChance) {
            int sideAWeight = 0;
            int sideBWeight = 0;
            CombatManager.Instance.GetCombatWeightsOfTwoLists(allies, enemies, out sideAWeight, out sideBWeight);
            int totalWeight = sideAWeight + sideBWeight;

            allyChance = (sideAWeight / (float) totalWeight) * 100f;
            enemyChance = (sideBWeight / (float) totalWeight) * 100f;
        }
        public void GetCombatWeightsOfTwoLists(List<ICharacter> allies, List<ICharacter> enemies, out int allyWeight, out int enemyWeight) {
            if(allies == null) {
                allyWeight = 0;
                enemyWeight = 1;
                return;
            }
            if (enemies == null) {
                allyWeight = 1;
                enemyWeight = 0;
                return;
            }
            int pairCombatCount = allies.Count;
            if (enemies.Count > allies.Count) {
                pairCombatCount = enemies.Count;
            }
            for (int i = 0; i < allies.Count; i++) {
                allies[i].pairCombatStats = new PairCombatStats[pairCombatCount];
            }
            for (int i = 0; i < enemies.Count; i++) {
                enemies[i].pairCombatStats = new PairCombatStats[pairCombatCount];
            }

            allyWeight = GetTotalWinWeight(allies, enemies);
            enemyWeight = GetTotalWinWeight(enemies, allies);
        }
        private int GetTotalWinWeight(List<ICharacter> icharacters1, List<ICharacter> icharacters2) {
            int totalWeight = 0;
            for (int i = 0; i < icharacters1.Count; i++) {
                ICharacter icharacter = icharacters1[i];

                ApplyBaseStats(icharacter);
                ApplyWinWeightOfCharacter(icharacter, icharacters1, icharacters2);

                //Calculate total power per pair
                for (int j = 0; j < icharacter.pairCombatStats.Length; j++) {
                    int totalFlatAttack = icharacter.combatBaseAttack + icharacter.combatAttackFlat + icharacter.pairCombatStats[j].attackFlat;
                    int totalFlatSpeed = icharacter.combatBaseSpeed + icharacter.combatSpeedFlat + icharacter.pairCombatStats[j].speedFlat;
                    int totalFlatHP = icharacter.combatBaseHP + icharacter.combatHPFlat + icharacter.pairCombatStats[j].hpFlat;
                    int totalFlatPower = icharacter.combatPowerFlat + icharacter.pairCombatStats[j].powerFlat;

                    int totalMultiplierAttack = icharacter.combatAttackMultiplier + icharacter.pairCombatStats[j].attackMultiplier;
                    int totalMultiplierSpeed = icharacter.combatSpeedMultiplier + icharacter.pairCombatStats[j].speedMultiplier;
                    int totalMultiplierHP = icharacter.combatHPMultiplier + icharacter.pairCombatStats[j].hpMultiplier;
                    int totalMultiplierPower = icharacter.combatPowerMultiplier + icharacter.pairCombatStats[j].powerMultiplier;

                    icharacter.pairCombatStats[j].totalPower =
                        (int)
                        //calculate total attack
                        ((totalFlatAttack + (totalFlatAttack * (totalMultiplierAttack / 100f))) +
                        //calculate total speed
                        (totalFlatSpeed + (totalFlatSpeed * (totalMultiplierSpeed / 100f))) +
                        //calculate total hp
                        (totalFlatHP + (totalFlatHP * (totalMultiplierHP / 100f))));

                    //Modify power
                    icharacter.pairCombatStats[j].totalPower += totalFlatPower;
                    icharacter.pairCombatStats[j].totalPower += (int) (icharacter.pairCombatStats[j].totalPower * (totalMultiplierPower / 100f));
                    totalWeight += icharacter.pairCombatStats[j].totalPower;
                }
            }
            return totalWeight;
        }
        private void ApplyWinWeightOfCharacter(ICharacter icharacter, List<ICharacter> allies, List<ICharacter> enemies) {
            for (int j = 0; j < icharacter.traits.Count; j++) {
                for (int k = 0; k < icharacter.traits[j].effects.Count; k++) {
                    TraitEffect traitEffect = icharacter.traits[j].effects[k];
                    ApplyTraitEffectPrePairing(icharacter, allies, enemies, traitEffect);
                    ApplyTraitEffectPairing(icharacter, enemies, traitEffect);
                }
            }
        }
        private void ApplyBaseStats(ICharacter character) {
            character.combatBaseAttack = character.attackPower;
            character.combatBaseSpeed = character.speed;
            character.combatBaseHP = character.hp;
        }

        private void ApplyTraitEffectPrePairing(ICharacter icharacter, List<ICharacter> allies, List<ICharacter> enemies, TraitEffect traitEffect) {
            //List<ICharacter> enemies = GetEnemyCharacters(icharacter.currentSide);
            //List<ICharacter> allies = GetAllyCharacters(icharacter.currentSide);

            if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF_ALL_ENEMIES) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < enemies.Count; i++) {
                        if (CharacterFitsTraitCriteria(enemies[i], traitEffect)) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < enemies.Count; i++) {
                        ModifyStat(icharacter, traitEffect);
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF_ALL_IN_COMBAT) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < enemies.Count; i++) {
                        if (CharacterFitsTraitCriteria(enemies[i], traitEffect)) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                    for (int i = 0; i < allies.Count; i++) {
                        if (CharacterFitsTraitCriteria(allies[i], traitEffect)) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < enemies.Count; i++) {
                        ModifyStat(icharacter, traitEffect);
                    }
                    for (int i = 0; i < allies.Count; i++) {
                        ModifyStat(icharacter, traitEffect);
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF_ALL_PARTY_MEMBERS) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < allies.Count; i++) {
                        if (CharacterFitsTraitCriteria(allies[i], traitEffect)) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < allies.Count; i++) {
                        ModifyStat(icharacter, traitEffect);
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF_OTHER_PARTY_MEMBERS) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < allies.Count; i++) {
                        if (allies[i] != icharacter && CharacterFitsTraitCriteria(allies[i], traitEffect)) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < allies.Count; i++) {
                        if (allies[i] != icharacter) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ALL_ENEMIES) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < enemies.Count; i++) {
                        if (CharacterFitsTraitCriteria(enemies[i], traitEffect)) {
                            ModifyStat(enemies[i], traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < enemies.Count; i++) {
                        ModifyStat(enemies[i], traitEffect);
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ALL_IN_COMBAT) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < enemies.Count; i++) {
                        if (CharacterFitsTraitCriteria(enemies[i], traitEffect)) {
                            ModifyStat(enemies[i], traitEffect);
                        }
                    }
                    for (int i = 0; i < allies.Count; i++) {
                        if (CharacterFitsTraitCriteria(allies[i], traitEffect)) {
                            ModifyStat(allies[i], traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < enemies.Count; i++) {
                        ModifyStat(enemies[i], traitEffect);
                    }
                    for (int i = 0; i < allies.Count; i++) {
                        ModifyStat(allies[i], traitEffect);
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ALL_PARTY_MEMBERS) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < allies.Count; i++) {
                        if (CharacterFitsTraitCriteria(allies[i], traitEffect)) {
                            ModifyStat(allies[i], traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < allies.Count; i++) {
                        ModifyStat(allies[i], traitEffect);
                    }
                }
            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.OTHER_PARTY_MEMBERS) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < allies.Count; i++) {
                        if (allies[i] != icharacter && CharacterFitsTraitCriteria(allies[i], traitEffect)) {
                            ModifyStat(allies[i], traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < allies.Count; i++) {
                        if (allies[i] != icharacter) {
                            ModifyStat(allies[i], traitEffect);
                        }
                    }
                }
            }
        }
        private void ApplyTraitEffectPairing(ICharacter icharacter, List<ICharacter> enemies, TraitEffect traitEffect) {
            //if there is no more enemy but there is still a pair slot, the values are zero
            for (int i = 0; i < icharacter.pairCombatStats.Length; i++) {
                if (i < enemies.Count) {
                    ICharacter enemy = enemies[i];
                    if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                        if (traitEffect.hasRequirement) {
                            if (CharacterFitsTraitCriteria(enemy, traitEffect)) {
                                if (traitEffect.isPercentage) {
                                    if (traitEffect.stat == STAT.ATTACK) {
                                        icharacter.pairCombatStats[i].attackMultiplier += (int) traitEffect.amount;
                                    } else if (traitEffect.stat == STAT.SPEED) {
                                        icharacter.pairCombatStats[i].speedMultiplier += (int) traitEffect.amount;
                                    } else if (traitEffect.stat == STAT.HP) {
                                        icharacter.pairCombatStats[i].hpMultiplier += (int) traitEffect.amount;
                                    } else if (traitEffect.stat == STAT.POWER) {
                                        icharacter.pairCombatStats[i].powerMultiplier += (int) traitEffect.amount;
                                    }
                                } else {
                                    if (traitEffect.stat == STAT.ATTACK) {
                                        icharacter.pairCombatStats[i].attackFlat += (int) traitEffect.amount;
                                    } else if (traitEffect.stat == STAT.SPEED) {
                                        icharacter.pairCombatStats[i].speedFlat += (int) traitEffect.amount;
                                    } else if (traitEffect.stat == STAT.HP) {
                                        icharacter.pairCombatStats[i].hpFlat += (int) traitEffect.amount;
                                    } else if (traitEffect.stat == STAT.POWER) {
                                        icharacter.pairCombatStats[i].powerFlat += (int) traitEffect.amount;
                                    }
                                }
                            }
                        } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ENEMY) {
                            if (traitEffect.hasRequirement) {
                                if (CharacterFitsTraitCriteria(enemy, traitEffect)) {
                                    if (traitEffect.isPercentage) {
                                        if (traitEffect.stat == STAT.ATTACK) {
                                            enemy.pairCombatStats[i].attackMultiplier += (int) traitEffect.amount;
                                        } else if (traitEffect.stat == STAT.SPEED) {
                                            enemy.pairCombatStats[i].speedMultiplier += (int) traitEffect.amount;
                                        } else if (traitEffect.stat == STAT.HP) {
                                            enemy.pairCombatStats[i].hpMultiplier += (int) traitEffect.amount;
                                        } else if (traitEffect.stat == STAT.POWER) {
                                            enemy.pairCombatStats[i].powerMultiplier += (int) traitEffect.amount;
                                        }
                                    } else {
                                        if (traitEffect.stat == STAT.ATTACK) {
                                            enemy.pairCombatStats[i].attackFlat += (int) traitEffect.amount;
                                        } else if (traitEffect.stat == STAT.SPEED) {
                                            enemy.pairCombatStats[i].speedFlat += (int) traitEffect.amount;
                                        } else if (traitEffect.stat == STAT.HP) {
                                            enemy.pairCombatStats[i].hpFlat += (int) traitEffect.amount;
                                        } else if (traitEffect.stat == STAT.POWER) {
                                            enemy.pairCombatStats[i].powerFlat += (int) traitEffect.amount;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void ModifyStat(ICharacter icharacter, TraitEffect traitEffect) {
            if (traitEffect.isPercentage) {
                if (traitEffect.stat == STAT.ATTACK) {
                    icharacter.combatAttackMultiplier += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.SPEED) {
                    icharacter.combatSpeedMultiplier += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.HP) {
                    icharacter.combatHPMultiplier += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.POWER) {
                    icharacter.combatPowerMultiplier += (int) traitEffect.amount;
                }
            } else {
                if (traitEffect.stat == STAT.ATTACK) {
                    icharacter.combatAttackFlat += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.SPEED) {
                    icharacter.combatSpeedFlat += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.HP) {
                    icharacter.combatHPFlat += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.POWER) {
                    icharacter.combatPowerFlat += (int) traitEffect.amount;
                }
            }

        }
        private bool CharacterFitsTraitCriteria(ICharacter icharacter, TraitEffect traitEffect) {
            if (traitEffect.requirementType == TRAIT_REQUIREMENT.RACE) {
                if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.AND) {
                    //if there is one mismatch, return false already because the separator is AND, otherwise, return true
                    if (traitEffect.isNot) {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetTrait(traitEffect.requirements[i]) != null) {
                                return false;
                            }
                        }
                        return true;
                    } else {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetTrait(traitEffect.requirements[i]) == null) {
                                return false;
                            }
                        }
                        return true;
                    }
                } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                    //if there is one match, return true already because the separator is OR, otherwise, return false   
                    if (traitEffect.isNot) {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetTrait(traitEffect.requirements[i]) == null) {
                                return true;
                            }
                        }
                        return false;
                    } else {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetTrait(traitEffect.requirements[i]) != null) {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.TRAIT) {
                if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.AND) {
                    //if there is one mismatch, return false already because the separator is AND, otherwise, return true
                    if (traitEffect.isNot) {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (traitEffect.requirements[i].ToLower() == icharacter.race.ToString().ToLower()) {
                                return false;
                            }
                        }
                        return true;
                    } else {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (traitEffect.requirements[i].ToLower() != icharacter.race.ToString().ToLower()) {
                                return false;
                            }
                        }
                        return true;
                    }
                } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                    //if there is one match, return true already because the separator is OR, otherwise, return false   
                    if (traitEffect.isNot) {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (traitEffect.requirements[i].ToLower() != icharacter.race.ToString().ToLower()) {
                                return true;
                            }
                        }
                        return false;
                    } else {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (traitEffect.requirements[i].ToLower() == icharacter.race.ToString().ToLower()) {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Roads Combat
        public SIDES GetOppositeSide(SIDES side) {
            if(side == SIDES.A) {
                return SIDES.B;
            }
            return SIDES.A;
        }
        #endregion
    }
}