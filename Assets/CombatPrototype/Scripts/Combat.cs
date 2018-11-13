using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

    public class Combat {
        internal List<ICharacter> charactersSideA;
        internal List<ICharacter> charactersSideB;
        internal List<Action> afterCombatActions;

        internal SIDES winningSide;

        internal List<string> resultsLog;

        public Combat(Party party1, Party party2) {
            this.charactersSideA = party1.icharacters;
            this.charactersSideB = party2.icharacters;
            for (int i = 0; i < party1.icharacters.Count; i++) {
                party1.icharacters[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
            }
            for (int i = 0; i < party2.icharacters.Count; i++) {
                party2.icharacters[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
            }
            this.afterCombatActions = new List<Action>();
            this.resultsLog = new List<string>();
        }

        public void Fight() {
            for (int i = 0; i < charactersSideA.Count; i++) {
                for (int j = 0; j < charactersSideA[i].combatAttributes.Count; j++) {

                }
            }
        }

        private void ApplyTraitEffectPrePairing(ICharacter icharacter, TraitEffect traitEffect) {
            List<ICharacter> enemies = GetEnemyCharacters(icharacter.currentSide);
            List<ICharacter> allies = GetAllyCharacters(icharacter.currentSide);

            if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF_ALL_ENEMIES) {
                if (traitEffect.hasRequirement) {
                    for (int i = 0; i < enemies.Count; i++) {
                        if(CharacterFitsTraitCriteria(enemies[i], traitEffect)) {
                            ModifyStat(icharacter, traitEffect);
                        }
                    }
                } else {
                    for (int i = 0; i < enemies.Count; i++) {
                        ModifyStat(icharacter, traitEffect);
                    }
                }
            }else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF_ALL_IN_COMBAT) {
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
            }
        }
        private void AddFlatStat(ICharacter icharacter, TraitEffect traitEffect) {
            if (traitEffect.stat == STAT.ATTACK) {
                icharacter.combatAttackFlat += (int) traitEffect.amount;
            } else if (traitEffect.stat == STAT.SPEED) {
                icharacter.combatSpeedFlat += (int) traitEffect.amount;
            } else if (traitEffect.stat == STAT.HP) {
                icharacter.combatHPFlat += (int) traitEffect.amount;
            }
        }
        private void AddPercentStat(ICharacter icharacter, TraitEffect traitEffect) {
            if (traitEffect.stat == STAT.ATTACK) {
                icharacter.combatAttackMultiplier += (int) traitEffect.amount;
            } else if (traitEffect.stat == STAT.SPEED) {
                icharacter.combatSpeedMultiplier += (int) traitEffect.amount;
            } else if (traitEffect.stat == STAT.HP) {
                icharacter.combatHPMultiplier += (int) traitEffect.amount;
            }
        }
        private bool CharacterFitsTraitCriteria(ICharacter icharacter, TraitEffect traitEffect) {
            if (traitEffect.requirementType == TRAIT_REQUIREMENT.RACE) {
                if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.AND) {
                    //if there is one mismatch, return false already because the separator is AND, otherwise, return true
                    if (traitEffect.isNot) {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetCombatAttribute(traitEffect.requirements[i]) != null) {
                                return false;
                            }
                        }
                        return true;
                    } else {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetCombatAttribute(traitEffect.requirements[i]) == null) {
                                return false;
                            }
                        }
                        return true;
                    }
                } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                    //if there is one match, return true already because the separator is OR, otherwise, return false   
                    if (traitEffect.isNot) {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetCombatAttribute(traitEffect.requirements[i]) == null) {
                                return true;
                            }
                        }
                        return false;
                    } else {
                        for (int i = 0; i < traitEffect.requirements.Count; i++) {
                            if (icharacter.GetCombatAttribute(traitEffect.requirements[i]) != null) {
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
        private void ApplyTraitEffectToOther(TraitEffect traitEffect) {

        }

        private List<ICharacter> GetEnemyCharacters(SIDES side) {
            if(side == SIDES.A) {
                return charactersSideB;
            }
            return charactersSideA;
        }
        private List<ICharacter> GetAllyCharacters(SIDES side) {
            if (side == SIDES.A) {
                return charactersSideA;
            }
            return charactersSideB;
        }

        public void AddAfterCombatAction(Action action) {
            afterCombatActions.Add(action);
        }
    }
}

