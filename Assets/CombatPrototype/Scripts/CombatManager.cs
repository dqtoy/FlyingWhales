using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Traits;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance = null;

    public float updateIntervals;
    public int numOfCombatActionPerDay;

    public Combat combat;
    public NewCombat newCombat;

    public CharacterSetup[] baseCharacters;
	public Color[] characterColors;

	private List<Color> unusedColors;
	private List<Color> usedColors;

    public const int pursueDuration = 10;

    private void Awake() {
        Instance = this;
    }
	internal void Initialize(){
        unusedColors = new List<Color>();
        usedColors = new List<Color>();
        newCombat.Initialize();
    }
    private void ConstructBaseCharacters() {
        string path = UtilityScripts.Utilities.dataPath + "CharacterSetups/";
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
    //internal Character CreateNewCharacter(CharacterSetup baseCharacter) {
    //    return new Character(baseCharacter, Utilities.GetRandomGender());
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

    #region Chance Combat
    public void GetCombatChanceOfTwoLists(List<Character> allies, List<Character> enemies, out float allyChance, out float enemyChance) {
        int sideAWeight = 0;
        int sideBWeight = 0;
        GetCombatWeightsOfTwoLists(allies, enemies, out sideAWeight, out sideBWeight);
        int totalWeight = sideAWeight + sideBWeight;
        
        allyChance = (sideAWeight / (float) totalWeight) * 100f;
        enemyChance = (sideBWeight / (float) totalWeight) * 100f;

        //Debug.Log("COMBAT " + allies[0].name + ": " + allyChance + "! " + enemies[0].name + ": " + enemyChance);
    }
    public void GetCombatWeightsOfTwoLists(List<Character> allies, List<Character> enemies, out int allyWeight, out int enemyWeight) {
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
        //int pairCombatCount = allies.Count;
        //if (enemies.Count > allies.Count) {
        //    pairCombatCount = enemies.Count;
        //}
        //for (int i = 0; i < allies.Count; i++) {
        //    allies[i].pairCombatStats = new PairCombatStats[pairCombatCount];
        //}
        //for (int i = 0; i < enemies.Count; i++) {
        //    enemies[i].pairCombatStats = new PairCombatStats[pairCombatCount];
        //}

        allyWeight = GetTotalWinWeight(allies, enemies);
        enemyWeight = GetTotalWinWeight(enemies, allies);
    }
    private int GetTotalWinWeight(List<Character> characters1, List<Character> characters2) {
        int totalWeight = 0;
        for (int i = 0; i < characters1.Count; i++) {
            Character character = characters1[i];

            float speedFormula = 1f + (character.speed / 200f);
            float totalPower = ((character.attackPower / 10f) * speedFormula) + ((character.maxHP / 30f) * speedFormula);

            if (characters2.Count == 1) {
                if (character.race == RACE.DRAGON) {
                    totalPower *= 1.25f;
                }
                if (character.isFactionLeader) {
                    totalPower *= 1.25f;
                }

                if (character.race == RACE.HUMANS) {
                    // if(characters2[0].role.roleType == CHARACTER_ROLE.BEAST) {
                    //     totalPower *= 1.25f;
                    // }
                } else if (character.race == RACE.ELVES) {
                    if (characters2[0].characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                        totalPower *= 1.25f;
                    }
                } else if (character.race == RACE.GOBLIN) {
                    if (characters2[0].characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        totalPower *= 0.85f;
                    }
                } else if (character.race == RACE.FAERY) {
                    if (characters2[0].characterClass.rangeType == RANGE_TYPE.MELEE) {
                        totalPower *= 1.25f;
                    }
                } else if (character.race == RACE.SKELETON) {
                    if (characters2[0].characterClass.rangeType == RANGE_TYPE.RANGED && characters2[0].characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        totalPower *= 0.75f;
                    }
                } else if (character.race == RACE.SPIDER) {
                    if (characters2[0].race == RACE.FAERY) {
                        totalPower *= 1.25f;
                    }
                } else if (character.race == RACE.WOLF) {
                    if (characters2[0].race == RACE.GOBLIN) {
                        totalPower *= 1.25f;
                    }
                } else if (character.race == RACE.GOLEM) {
                    if (characters2[0].race == RACE.ELVES) {
                        totalPower *= 1.25f;
                    }
                }

                if (character.characterClass.rangeType == RANGE_TYPE.MELEE && character.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                    if (characters2[0].characterClass.rangeType == RANGE_TYPE.RANGED && characters2[0].characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        totalPower *= 1.25f;
                    }
                } else if (character.characterClass.rangeType == RANGE_TYPE.RANGED && character.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                    if (characters2[0].characterClass.rangeType == RANGE_TYPE.RANGED && characters2[0].characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                        totalPower *= 1.25f;
                    }
                } else if (character.characterClass.rangeType == RANGE_TYPE.RANGED && character.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                    if (characters2[0].characterClass.rangeType == RANGE_TYPE.MELEE && characters2[0].characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        totalPower *= 1.25f;
                    }
                }
            }

            totalWeight += (int) totalPower;
        }
        return totalWeight;
    }
    //private void ApplyWinWeightOfCharacter(Character icharacter, List<Character> allies, List<Character> enemies) {
    //    for (int j = 0; j < icharacter.traits.Count; j++) {
    //        for (int k = 0; k < icharacter.traits[j].effects.Count; k++) {
    //            TraitEffect traitEffect = icharacter.traits[j].effects[k];
    //            ApplyTraitEffectPrePairing(icharacter, allies, enemies, traitEffect);
    //            ApplyTraitEffectPairing(icharacter, enemies, traitEffect);
    //        }
    //    }
    //}
    //private void ApplyBaseStats(Character character) {
    //    character.combatBaseAttack = character.attackPower;
    //    character.combatBaseSpeed = character.speed;
    //    character.combatBaseHP = character.maxHP;
    //}

    //private void ApplyTraitEffectPairing(Character icharacter, List<Character> enemies, TraitEffect traitEffect) {
        //if there is no more enemy but there is still a pair slot, the values are zero
        //for (int i = 0; i < icharacter.pairCombatStats.Length; i++) {
        //    if (i < enemies.Count) {
        //        Character enemy = enemies[i];
        //        if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
        //            if (traitEffect.hasRequirement) {
        //                if (CharacterFitsTraitCriteria(enemy, traitEffect)) {
        //                    if (traitEffect.isPercentage) {
        //                        if (traitEffect.stat == STAT.ATTACK) {
        //                            icharacter.pairCombatStats[i].attackMultiplier += (int) traitEffect.amount;
        //                        } else if (traitEffect.stat == STAT.SPEED) {
        //                            icharacter.pairCombatStats[i].speedMultiplier += (int) traitEffect.amount;
        //                        } else if (traitEffect.stat == STAT.HP) {
        //                            icharacter.pairCombatStats[i].hpMultiplier += (int) traitEffect.amount;
        //                        } else if (traitEffect.stat == STAT.POWER) {
        //                            icharacter.pairCombatStats[i].powerMultiplier += (int) traitEffect.amount;
        //                        }
        //                    } else {
        //                        if (traitEffect.stat == STAT.ATTACK) {
        //                            icharacter.pairCombatStats[i].attackFlat += (int) traitEffect.amount;
        //                        } else if (traitEffect.stat == STAT.SPEED) {
        //                            icharacter.pairCombatStats[i].speedFlat += (int) traitEffect.amount;
        //                        } else if (traitEffect.stat == STAT.HP) {
        //                            icharacter.pairCombatStats[i].hpFlat += (int) traitEffect.amount;
        //                        } else if (traitEffect.stat == STAT.POWER) {
        //                            icharacter.pairCombatStats[i].powerFlat += (int) traitEffect.amount;
        //                        }
        //                    }
        //                }
        //            } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ENEMY) {
        //                if (traitEffect.hasRequirement) {
        //                    if (CharacterFitsTraitCriteria(enemy, traitEffect)) {
        //                        if (traitEffect.isPercentage) {
        //                            if (traitEffect.stat == STAT.ATTACK) {
        //                                enemy.pairCombatStats[i].attackMultiplier += (int) traitEffect.amount;
        //                            } else if (traitEffect.stat == STAT.SPEED) {
        //                                enemy.pairCombatStats[i].speedMultiplier += (int) traitEffect.amount;
        //                            } else if (traitEffect.stat == STAT.HP) {
        //                                enemy.pairCombatStats[i].hpMultiplier += (int) traitEffect.amount;
        //                            } else if (traitEffect.stat == STAT.POWER) {
        //                                enemy.pairCombatStats[i].powerMultiplier += (int) traitEffect.amount;
        //                            }
        //                        } else {
        //                            if (traitEffect.stat == STAT.ATTACK) {
        //                                enemy.pairCombatStats[i].attackFlat += (int) traitEffect.amount;
        //                            } else if (traitEffect.stat == STAT.SPEED) {
        //                                enemy.pairCombatStats[i].speedFlat += (int) traitEffect.amount;
        //                            } else if (traitEffect.stat == STAT.HP) {
        //                                enemy.pairCombatStats[i].hpFlat += (int) traitEffect.amount;
        //                            } else if (traitEffect.stat == STAT.POWER) {
        //                                enemy.pairCombatStats[i].powerFlat += (int) traitEffect.amount;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    //}
    //private void ModifyStat(Character icharacter, TraitEffect traitEffect) {
    //    if (traitEffect.isPercentage) {
    //        if (traitEffect.stat == STAT.ATTACK) {
    //            icharacter.combatAttackMultiplier += (int) traitEffect.amount;
    //        } else if (traitEffect.stat == STAT.SPEED) {
    //            icharacter.combatSpeedMultiplier += (int) traitEffect.amount;
    //        } else if (traitEffect.stat == STAT.HP) {
    //            icharacter.combatHPMultiplier += (int) traitEffect.amount;
    //        } else if (traitEffect.stat == STAT.POWER) {
    //            icharacter.combatPowerMultiplier += (int) traitEffect.amount;
    //        }
    //    } else {
    //        if (traitEffect.stat == STAT.ATTACK) {
    //            icharacter.combatAttackFlat += (int) traitEffect.amount;
    //        } else if (traitEffect.stat == STAT.SPEED) {
    //            icharacter.combatSpeedFlat += (int) traitEffect.amount;
    //        } else if (traitEffect.stat == STAT.HP) {
    //            icharacter.combatHPFlat += (int) traitEffect.amount;
    //        } else if (traitEffect.stat == STAT.POWER) {
    //            icharacter.combatPowerFlat += (int) traitEffect.amount;
    //        }
    //    }
    //}
    private List<Character> GetCharactersToBeChecked(TRAIT_REQUIREMENT_CHECKER checker, Character sourceCharacter,
       List<Character> allies, List<Character> enemies, List<Character> targets = null) {

        List<Character> checkerCharacters = new List<Character>();
        if (checker == TRAIT_REQUIREMENT_CHECKER.SELF) {
            checkerCharacters.Add(sourceCharacter);
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.ENEMY) {
            checkerCharacters.AddRange(targets);
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.OTHER_PARTY_MEMBERS) {
            checkerCharacters = allies.Where(x => x != sourceCharacter).ToList();
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.ALL_PARTY_MEMBERS) {
            checkerCharacters.AddRange(allies);
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.ALL_IN_COMBAT) {
            checkerCharacters.AddRange(allies);
            checkerCharacters.AddRange(enemies);
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.ALL_ENEMIES) {
            checkerCharacters.AddRange(enemies);
        }
        return checkerCharacters;
    }
    private bool CharacterFitsTraitCriteria(Character icharacter, TraitEffect traitEffect) {
        if (traitEffect.requirementType == TRAIT_REQUIREMENT.TRAIT) {
            if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.AND) {
                //if there is one mismatch, return false already because the separator is AND, otherwise, return true
                if (traitEffect.isNot) {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (icharacter.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (!icharacter.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return false;
                        }
                    }
                    return true;
                }
            } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                //if there is one match, return true already because the separator is OR, otherwise, return false   
                if (traitEffect.isNot) {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (!icharacter.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return true;
                        }
                    }
                    return false;
                } else {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (icharacter.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return true;
                        }
                    }
                    return false;
                }
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.RACE) {
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
    public int GetAdjacentIndexGrid(int index) {
        if(index == 0) {
            return 1;
        }else if (index == 1) {
            return 0;
        }else if (index == 2) {
            return 3;
        }else if (index == 3) {
            return 2;
        }
        return index;
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