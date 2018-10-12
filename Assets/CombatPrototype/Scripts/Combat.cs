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

    public class Combat : Multithread {
        //		public Dictionary<SIDES, List<ICharacter>> allCharactersAndSides;
        public delegate void Results();
        public static event Results CheckResults;

        internal List<ICharacter> charactersSideA;
        internal List<ICharacter> charactersSideB;
        internal List<ICharacter> deadCharacters;
        internal List<ICharacter> faintedCharacters;
        internal List<ICharacter> fledCharacters;
        internal List<ICharacter> characterSideACopy;
        internal List<ICharacter> characterSideBCopy;
        internal List<Action> afterCombatActions;

        internal SIDES winningSide;

        internal List<string> resultsLog;
        //internal ILocation location;
        internal bool isDone;
        internal bool hasStarted;

        public Combat() {
            //			this.allCharactersAndSides = new Dictionary<SIDES, List<ICharacter>> ();
            this.charactersSideA = new List<ICharacter>();
            this.charactersSideB = new List<ICharacter>();
            this.characterSideACopy = new List<ICharacter>();
            this.characterSideBCopy = new List<ICharacter>();
            this.deadCharacters = new List<ICharacter>();
            //this.faintedCharacters = new List<ICharacter> ();
            this.fledCharacters = new List<ICharacter>();
            this.afterCombatActions = new List<Action>();
            //this.location = location;
            this.isDone = false;
            this.hasStarted = false;

            this.resultsLog = new List<string>();
            //			Messenger.AddListener<ICharacter> ("CharacterDeath", CharacterDeath);
        }

        #region ICharacter Management
        //Add a character to a side
        internal void AddCharacter(SIDES side, ICharacter character) {
            if (!this.charactersSideA.Contains(character) && !this.charactersSideB.Contains(character)) {
                int rowNumber = 1;
                if (side == SIDES.A) {
                    this.charactersSideA.Add(character);
                    this.characterSideACopy.Add(character);
                } else {
                    this.charactersSideB.Add(character);
                    this.characterSideBCopy.Add(character);
                    rowNumber = 5;
                }
                if (character is Character) {
                    (character as Character).SetDoNotDisturb(true);
                }
                if (character.ownParty is CharacterParty) {
                    (character.ownParty as CharacterParty).actionData.SetIsHalted(true);
                }
                character.ownParty.currentCombat = this;
                character.SetSide(side);
                //character.currentCombat = this;
                character.SetRowNumber(rowNumber);
                character.actRate = 0;
                character.battleOnlyTracker.Reset();
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
        internal void AddCharacters(SIDES side, List<ICharacter> characters) {
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
                characters[i].SetSide(side);
                if (characters[i] is Character) {
                    (characters[i] as Character).SetDoNotDisturb(true);
                }
                //characters[i].currentCombat = this;
                characters[i].SetRowNumber(rowNumber);
                characters[i].actRate = 0;
                if (hasStarted && !isDone) {
                    string log = characters[i].coloredUrlName + " joins the battle on Side " + side.ToString();
                    Debug.Log(log);
                    AddCombatLog(log, side);
                }
            }
            if (CombatPrototypeUI.Instance != null) {
                CombatPrototypeUI.Instance.UpdateCharactersList(side);
            }
        }
        internal void AddParty(SIDES side, Party iparty) {
            for (int i = 0; i < iparty.icharacters.Count; i++) {
                ICharacter currChar = iparty.icharacters[i];
                AddCharacter(side, currChar);
            }
            //iparty.currentCombat = this;
            //if (iparty is CharacterParty) {
            //    (iparty as CharacterParty).actionData.SetIsHalted(true);
            //}
        }
        //Remove a character from a side
        internal bool RemoveCharacter(SIDES side, ICharacter character) {
            if (side == SIDES.A) {
                if (this.charactersSideA.Remove(character)) {
                    if (character is Character) {
                        (character as Character).SetDoNotDisturb(false);
                    }
                    return true;
                }
            } else {
                if (this.charactersSideB.Remove(character)) {
                    if (character is Character) {
                        (character as Character).SetDoNotDisturb(false);
                    }
                    return true;
                }
            }
            //character.currentCombat = null;
            //if (CombatPrototypeUI.Instance != null) {
            //    CombatPrototypeUI.Instance.UpdateCharactersList(side);
            //}
            return false;
        }
        //Remove character without specifying a side
        internal bool RemoveCharacter(ICharacter character) {
            if (this.charactersSideA.Remove(character)) {
                if (character is Character) {
                    (character as Character).SetDoNotDisturb(false);
                }
                //character.currentCombat = null;
                if (CombatPrototypeUI.Instance != null) {
                    CombatPrototypeUI.Instance.UpdateCharactersList(SIDES.A);
                }
                return true;
            } else if (this.charactersSideB.Remove(character)) {
                if (character is Character) {
                    (character as Character).SetDoNotDisturb(false);
                }
                //character.currentCombat = null;
                if (CombatPrototypeUI.Instance != null) {
                    CombatPrototypeUI.Instance.UpdateCharactersList(SIDES.B);
                }
                return true;
            }
            return false;
        }
        internal List<ICharacter> GetCharactersOnSide(SIDES side) {
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
        public void ReturnCombatResults() {
            CombatManager.Instance.CombatResults(this);
            //if (attacker != null) {
            //    attacker.ReturnCombatResults(this);
            //}
            //if (defender != null) {
            //    defender.ReturnCombatResults(this);
            //}
        }

        //This simulates the whole combat system
        public void CombatSimulation() {
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
            //SetRowNumber (this.charactersSideA, 1);
            //SetRowNumber (this.charactersSideB, 5);

            int rounds = 1;
            while (this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0) {
                Debug.Log("========== Round " + rounds.ToString() + " ==========");
                ICharacter characterThatWillAct = GetCharacterThatWillAct(this.charactersSideA, this.charactersSideB);
                ICharacter targetCharacter = GetTargetCharacter(characterThatWillAct, null);

                Character actingCharacter = null;
                if (characterThatWillAct.icharacterType == ICHARACTER_TYPE.CHARACTER) {
                    actingCharacter = characterThatWillAct as Character;
                }
                Debug.Log((actingCharacter != null ? actingCharacter.characterClass.className : "") + characterThatWillAct.name + " will act. (hp lost: " + characterThatWillAct.battleOnlyTracker.hpLostPercent
                    + ", last damage taken: " + characterThatWillAct.battleOnlyTracker.lastDamageTaken);
                Debug.Log((targetCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER ? (targetCharacter as Character).characterClass.className : "") + targetCharacter.name + " is the target. (hp lost: " + targetCharacter.battleOnlyTracker.hpLostPercent
                    + ", last damage taken: " + targetCharacter.battleOnlyTracker.lastDamageTaken);

                characterThatWillAct.EnableDisableSkills(this);
                //Debug.Log("Available Skills: ");
                //for (int i = 0; i < characterThatWillAct.skills.Count; i++) {
                //    Skill currSkill = characterThatWillAct.skills[i];
                //    if (currSkill.isEnabled) {
                //        Debug.Log(currSkill.skillName);
                //    }
                //}
                Skill skillToUse = GetSkillToUse(characterThatWillAct, targetCharacter);
                if (skillToUse != null) {
                    Debug.Log(characterThatWillAct.name + " decides to use " + skillToUse.skillName);
                    if (actingCharacter != null) {
                        actingCharacter.CureStatusEffects();
                    }
                    //ICharacter targetCharacter = GetTargetCharacter(characterThatWillAct, skillToUse);
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
            } else {
                winningSide = SIDES.B;
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
        private void SetRowNumber(List<ICharacter> characters, int rowNumber) {
            for (int i = 0; i < characters.Count; i++) {
                characters[i].SetRowNumber(rowNumber);
            }
        }

        //Return a character that will act from a pool of characters based on their act rate
        private ICharacter GetCharacterThatWillAct(List<ICharacter> charactersSideA, List<ICharacter> charactersSideB) {
            //characterActivationWeights.Clear();
            //for (int i = 0; i < charactersSideA.Count; i++) {
            //    characterActivationWeights.Add(charactersSideA[i], charactersSideA[i].actRate);
            //}
            //for (int i = 0; i < charactersSideB.Count; i++) {
            //    characterActivationWeights.Add(charactersSideB[i], charactersSideB[i].actRate);
            //}

            //ICharacter chosenCharacter = Utilities.PickRandomElementWithWeights<ICharacter>(characterActivationWeights);
            //foreach (ICharacter character in characterActivationWeights.Keys) {
            //    character.actRate += character.speed;
            //}
            //chosenCharacter.actRate = chosenCharacter.speed;
            List<ICharacter> candidates = new List<ICharacter>();
            for (int i = 0; i < charactersSideA.Count; i++) {
                charactersSideA[i].actRate += charactersSideA[i].speed;
                if (charactersSideA[i].actRate >= 1000f) {
                    candidates.Add(charactersSideA[i]);
                }
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                charactersSideB[i].actRate += charactersSideB[i].speed;
                if (charactersSideA[i].actRate >= 1000f) {
                    candidates.Add(charactersSideB[i]);
                }
            }
            if (candidates.Count > 0) {
                ICharacter chosenCharacter = null;
                for (int i = 0; i < candidates.Count; i++) {
                    if (chosenCharacter == null) {
                        chosenCharacter = candidates[i];
                    } else {
                        if (candidates[i].actRate > chosenCharacter.actRate) {
                            chosenCharacter = candidates[i];
                        }
                    }
                }
                chosenCharacter.actRate = 0;
                return chosenCharacter;
            }
            return null;
        }
        private void PickCharacterToAct(List<ICharacter> charactersSideA, List<ICharacter> charactersSideB) {

        }

        //Get a random character from the opposite side to be the target
        private ICharacter GetTargetCharacter(ICharacter sourceCharacter, Skill skill) {
            //List<ICharacter> possibleTargets = new List<ICharacter>();
            List<ICharacter> oppositeTargets = this.charactersSideB;
            if (sourceCharacter.currentSide == SIDES.B) {
                oppositeTargets = this.charactersSideA;
            }
            return oppositeTargets[Utilities.rng.Next(0, oppositeTargets.Count)];
        }

        //Get Skill that the character will use based on activation weights, target character must be within skill range
        private Skill GetSkillToUse(ICharacter sourceCharacter, ICharacter targetCharacter = null) {
            Debug.Log("Available Skills: " + sourceCharacter.skills[0].skillName);
            return sourceCharacter.skills[0];
            //Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int> ();
            //for (int i = 0; i < sourceCharacter.skills.Count; i++) { //These are general skills like move, flee, drink potion
            //    Skill skill = sourceCharacter.skills[i];
            //    if (skill.isEnabled && skill.skillType != SKILL_TYPE.ATTACK) {
            //        skillActivationWeights.Add(skill, skill.activationWeight);
            //    }
            //}


            //float weaponAttack = 0f;
            //float missingHP = (1f - ((float) sourceCharacter.currentHP / (float) sourceCharacter.maxHP)) * 100f;
            //int levelDiff = (sourceCharacter.level - targetCharacter.level) * 10;
            //if (sourceCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            //    Character character = sourceCharacter as Character;
            //    if (character.equippedWeapon != null && sourceCharacter.battleOnlyTracker.lastDamageTaken < sourceCharacter.currentHP) {//character must have a weapon and sourceCharacter last damage taken must not be >= current health
            //        weaponAttack = character.equippedWeapon.attackPower;
            //        for (int i = 0; i < character.level; i++) {
            //            if(i < character.characterClass.skillsPerLevel.Count) {
            //                if(character.characterClass.skillsPerLevel[i] != null) {
            //                    for (int j = 0; j < character.characterClass.skillsPerLevel[i].Length; j++) {
            //                        Skill skill = character.characterClass.skillsPerLevel[i][j];
            //                        if (skill.isEnabled && skill.skillType == SKILL_TYPE.ATTACK) {
            //                            Debug.Log(skill.skillName);
            //                            AttackSkill attackSkill = skill as AttackSkill;
            //                            float initialWeight = GetSkillInitialWeight(sourceCharacter, targetCharacter, attackSkill, weaponAttack, missingHP, levelDiff, character.battleTracker);
            //                            float specialModifier = GetSpecialModifier(sourceCharacter, targetCharacter, attackSkill, character.battleTracker);
            //                            int finalWeight = Mathf.CeilToInt(initialWeight * (specialModifier / 100f));
            //                            if (finalWeight > 0) {
            //                                skillActivationWeights.Add(attackSkill, finalWeight);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //} else if (sourceCharacter.icharacterType == ICHARACTER_TYPE.MONSTER) {
            //    Monster monster = sourceCharacter as Monster;
            //    weaponAttack = monster.attackPower;
            //    for (int i = 0; i < monster.skills.Count; i++) {
            //        Skill skill = monster.skills[i];
            //        if (skill.isEnabled && skill.skillType == SKILL_TYPE.ATTACK) {
            //            Debug.Log(skill.skillName);
            //            AttackSkill attackSkill = skill as AttackSkill;
            //            float initialWeight = (float) attackSkill.activationWeight; // GetSkillInitialWeight(sourceCharacter, targetCharacter, attackSkill, weaponAttack, missingHP, levelDiff);
            //            float specialModifier = GetSpecialModifier(sourceCharacter, targetCharacter, attackSkill);
            //            int finalWeight = Mathf.CeilToInt(initialWeight * (specialModifier / 100f));
            //            if(finalWeight > 0) {
            //                skillActivationWeights.Add(attackSkill, finalWeight);
            //            }
            //        }
            //    }
            //}

            //if (skillActivationWeights.Count > 0) {
            //    Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill>(skillActivationWeights);
            //    return chosenSkill;
            //}
            //return null;
        }

        //private float GetSkillInitialWeight(ICharacter sourceCharacter, ICharacter targetCharacter, AttackSkill attackSkill, float weaponAttack, float missingHP, int levelDiff, CharacterBattleTracker battleTracker = null) {
        //    //int statUsed = attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL ? sourceCharacter.strength : sourceCharacter.intelligence;
        //    int finalAttack = 0;
        //    if(attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL) {
        //        finalAttack = sourceCharacter.pFinalAttack;
        //    } else {
        //        finalAttack = sourceCharacter.mFinalAttack;
        //    }
        //    float rawDamage = ((float) finalAttack * ((float) attackSkill.power / 100f)) * 1; //Subject to change: the *1 is the targets that will hit, this will change in the future

        //    float modifier = GetModifier(sourceCharacter, targetCharacter, attackSkill, rawDamage, missingHP, levelDiff, battleTracker);
        //    float spModifier = sourceCharacter.currentSP - attackSkill.spCost;
        //    if (spModifier <= 0f) { spModifier = 1f; }
        //    return (rawDamage * (1f + (modifier / 100f))) / spModifier;
        //}
        //private float GetModifier(ICharacter sourceCharacter, ICharacter targetCharacter, AttackSkill attackSkill, float rawDamage, float missingHP, int levelDiff, CharacterBattleTracker battleTracker = null) {
        //    //Elemental Weakness
        //    float elementalWeakness = 0f;
        //    if (attackSkill.element != ELEMENT.NONE && battleTracker != null) {
        //        if (battleTracker.tracker.ContainsKey(targetCharacter.name)) {
        //            if (battleTracker.tracker[targetCharacter.name].elementalWeaknesses.Contains(attackSkill.element)) {
        //                elementalWeakness = targetCharacter.elementalWeaknesses[attackSkill.element];
        //            }
        //        }
        //    }

        //    //Missing HP from parameter
        //    //Level Difference from parameter

        //    //HP Lost
        //    float hpLostPercent = sourceCharacter.battleOnlyTracker.hpLostPercent;

        //    //Expected Value
        //    float previousActualDamage = battleTracker != null && battleTracker.tracker.ContainsKey(targetCharacter.name) ? battleTracker.tracker[targetCharacter.name].lastDamageDealt : 0f;
        //    float expectedValue = previousActualDamage / (rawDamage + previousActualDamage);

        //    return elementalWeakness + missingHP + levelDiff + hpLostPercent + expectedValue;
        //}
        //private float GetSpecialModifier(ICharacter sourceCharacter, ICharacter targetCharacter, AttackSkill attackSkill, CharacterBattleTracker battleTracker = null) {
        //    //Attack misses per skill
        //    float attackMissPercent = (-33.4f) * (float)(sourceCharacter.battleOnlyTracker.consecutiveAttackMisses.ContainsKey(attackSkill.skillName) ? sourceCharacter.battleOnlyTracker.consecutiveAttackMisses[attackSkill.skillName] : 0);

        //    //Elemental Resist
        //    float resistance = 0f;
        //    if (attackSkill.element != ELEMENT.NONE && battleTracker != null) {
        //        if (battleTracker.tracker.ContainsKey(targetCharacter.name)) {
        //            if (battleTracker.tracker[targetCharacter.name].elementalResistances.Contains(attackSkill.element)) {
        //                resistance = targetCharacter.elementalResistances[attackSkill.element];
        //            }
        //        }
        //    }
        //    float elementalResistance = (100f - resistance) / 100f;

        //    //Will die next hit
        //    float dieNextHit = sourceCharacter.currentHP - sourceCharacter.battleOnlyTracker.lastDamageTaken;

        //    return attackMissPercent + resistance + elementalResistance + dieNextHit;
        //}

        //Check if there are targets in range for the specific skill so that the character can know if the skill can be activated 
        internal bool HasTargetInRangeForSkill(Skill skill, ICharacter sourceCharacter) {
            if (skill is AttackSkill) {
                if (sourceCharacter.currentSide == SIDES.A) {
                    for (int i = 0; i < this.charactersSideB.Count; i++) {
                        ICharacter targetCharacter = this.charactersSideB[i];
                        int rowDistance = GetRowDistanceBetweenTwoCharacters(sourceCharacter, targetCharacter);
                        if (skill.range >= rowDistance) {
                            return true;
                        }
                    }
                } else {
                    for (int i = 0; i < this.charactersSideA.Count; i++) {
                        ICharacter targetCharacter = this.charactersSideA[i];
                        int rowDistance = GetRowDistanceBetweenTwoCharacters(sourceCharacter, targetCharacter);
                        if (skill.range >= rowDistance) {
                            return true;
                        }
                    }
                }
                return false;
            } else {
                return true;
            }

        }

        internal bool HasTargetInRangeForSkill(SKILL_TYPE skillType, ICharacter sourceCharacter) {
            if (skillType == SKILL_TYPE.ATTACK) {
                for (int i = 0; i < sourceCharacter.skills.Count; i++) {
                    Skill skill = sourceCharacter.skills[i];
                    if (skill is AttackSkill) {
                        return HasTargetInRangeForSkill(skill, sourceCharacter);
                    }
                }
                if (sourceCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
                    Character character = sourceCharacter as Character;
                    for (int i = 0; i < character.level; i++) {
                        if (i < character.characterClass.skillsPerLevel.Count) {
                            if (character.characterClass.skillsPerLevel[i] != null) {
                                for (int j = 0; j < character.characterClass.skillsPerLevel[i].Length; j++) {
                                    Skill skill = character.characterClass.skillsPerLevel[i][j];
                                    if (skill is AttackSkill) {
                                        return HasTargetInRangeForSkill(skill, sourceCharacter);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        //Returns the row distance/difference of two characters
        private int GetRowDistanceBetweenTwoCharacters(ICharacter sourceCharacter, ICharacter targetCharacter) {
            int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
            if (distance < 0) {
                distance *= -1;
            }
            return distance;
        }
        //ICharacter will do the skill specified, but its success will be determined by the skill's accuracy
        private void DoSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter) {
            //If skill is attack, reduce sp
            if (skill.skillType == SKILL_TYPE.ATTACK) {
                AttackSkill attackSkill = skill as AttackSkill;
                sourceCharacter.AdjustSP(-attackSkill.spCost);
            }
            SuccessfulSkill(skill, sourceCharacter, targetCharacter);
        }

        //Go here if skill is accurate and is successful
        private void SuccessfulSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter) {
            if (skill is AttackSkill) {
                AttackSkill(skill, sourceCharacter, targetCharacter);
            } else if (skill is HealSkill) {
                HealSkill(skill, sourceCharacter, targetCharacter);
            } else if (skill is FleeSkill) {
                targetCharacter = sourceCharacter;
                FleeSkill(sourceCharacter, targetCharacter);
            } else if (skill is ObtainSkill) {
                ObtainItemSkill(sourceCharacter, targetCharacter);
            } else if (skill is MoveSkill) {
                targetCharacter = sourceCharacter;
                MoveSkill(skill, sourceCharacter, targetCharacter);
            }
        }

        //Skill is not accurate and therefore has failed to execute
        private void FailedSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter) {
            //TODO: What happens when a skill has failed?
            if (skill is FleeSkill) {
                AddCombatLog(sourceCharacter.coloredUrlName + " tried to flee but got tripped over and fell down!", sourceCharacter.currentSide);
                //				CounterAttack (targetCharacter);
            } else if (skill is AttackSkill) {
                sourceCharacter.battleOnlyTracker.AddAttackMiss(skill.skillName, 1);
                AddCombatLog(sourceCharacter.coloredUrlName + " tried to " + skill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " but missed!", sourceCharacter.currentSide);
            } else if (skill is HealSkill) {
                if (sourceCharacter == targetCharacter) {
                    AddCombatLog(sourceCharacter.coloredUrlName + " tried to use " + skill.skillName.ToLower() + " to heal himself/herself but it is already expired!", sourceCharacter.currentSide);
                } else {
                    AddCombatLog(sourceCharacter.coloredUrlName + " tried to use " + skill.skillName.ToLower() + " to heal " + targetCharacter.coloredUrlName + " but it is already expired!", sourceCharacter.currentSide);
                }
            }
        }

        //Get DEFEND_TYPE for the attack skill, if DEFEND_TYPE is NONE, then target character has not defend successfully, therefore, the target character will be damaged
        //private DEFEND_TYPE CanTargetCharacterDefend(ICharacter sourceCharacter, ICharacter targetCharacter){
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

        private void CounterAttack(ICharacter character) {
            //TODO: Counter attack
            AddCombatLog(character.coloredUrlName + " counterattacked!", character.currentSide);
        }

        private void InstantDeath(ICharacter character, ICharacter sourceCharacter) {
            character.FaintOrDeath(sourceCharacter);
        }

        #region Attack Skill
        private void AttackSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter) {
            AttackSkill attackSkill = skill as AttackSkill;
            HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter);
        }

        //Hits the target with an attack skill
        private void HitTargetCharacter(AttackSkill attackSkill, ICharacter sourceCharacter, ICharacter targetCharacter) {
            //Total Damage = [Weapon Power + (Int or Str)] - [Base Damage Mitigation] - [Bonus Attack Type Mitigation] + [Bonus Attack Type Weakness]
            string log = string.Empty;
            float attackPower = sourceCharacter.attackPower;
            if(sourceCharacter.combatAttributes != null) {
                //Apply all flat damage attack power modifier first
                for (int i = 0; i < sourceCharacter.combatAttributes.Count; i++) {
                    if(!sourceCharacter.combatAttributes[i].isPercentage && sourceCharacter.combatAttributes[i].stat == STAT.ATTACK && sourceCharacter.combatAttributes[i].hasRequirement
                        && sourceCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.DEALT) {
                        if(IsCombatAttributeApplicable(sourceCharacter.combatAttributes[i], targetCharacter, attackSkill)) {
                            attackPower += sourceCharacter.combatAttributes[i].amount;
                        }
                    }
                }
                for (int i = 0; i < targetCharacter.combatAttributes.Count; i++) {
                    if (!targetCharacter.combatAttributes[i].isPercentage && targetCharacter.combatAttributes[i].stat == STAT.ATTACK && targetCharacter.combatAttributes[i].hasRequirement
                        && targetCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.RECEIVED) {
                        if (IsCombatAttributeApplicable(targetCharacter.combatAttributes[i], sourceCharacter, attackSkill)) {
                            attackPower += targetCharacter.combatAttributes[i].amount;
                        }
                    }
                }

                //Then apply all percentage modifiers
                for (int i = 0; i < sourceCharacter.combatAttributes.Count; i++) {
                    if (sourceCharacter.combatAttributes[i].isPercentage && sourceCharacter.combatAttributes[i].stat == STAT.ATTACK && sourceCharacter.combatAttributes[i].hasRequirement
                        && sourceCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.DEALT) {
                        if (IsCombatAttributeApplicable(sourceCharacter.combatAttributes[i], targetCharacter, attackSkill)) {
                            float result = attackPower * (sourceCharacter.combatAttributes[i].amount / 100f);
                            attackPower += result;
                        }
                    }
                }
                for (int i = 0; i < targetCharacter.combatAttributes.Count; i++) {
                    if (targetCharacter.combatAttributes[i].isPercentage && targetCharacter.combatAttributes[i].stat == STAT.ATTACK && targetCharacter.combatAttributes[i].hasRequirement
                        && targetCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.RECEIVED) {
                        if (IsCombatAttributeApplicable(targetCharacter.combatAttributes[i], sourceCharacter, attackSkill)) {
                            float result = attackPower * (targetCharacter.combatAttributes[i].amount / 100f);
                            attackPower += result;
                        }
                    }
                }
            }
            int damage = (int) attackPower;
            log += sourceCharacter.coloredUrlName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + "(" + damage.ToString() + ")"; ;
            AddCombatLog(log, sourceCharacter.currentSide);

            targetCharacter.AdjustHP(-damage, sourceCharacter);

            ////Reset attack miss
            //sourceCharacter.battleOnlyTracker.ResetAttackMiss(attackSkill.skillName);
            //Character attacker = null;
            //Weapon weapon = null;
            //float damageRange = 0f;
            //int statMod = sourceCharacter.strength;
            //int def = targetCharacter.GetDef();
            //float critDamage = 100f;
            //if (sourceCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            //    attacker = sourceCharacter as Character;
            //    weapon = attacker.equippedWeapon;
            //}
            //if (attackSkill.attackCategory == ATTACK_CATEGORY.MAGICAL) {
            //    statMod = sourceCharacter.intelligence;
            //}
            //int critChance = Utilities.rng.Next(0, 100);
            //if (critChance < sourceCharacter.critChance) {
            //    //CRITICAL HIT!
            //    Debug.Log(attackSkill.skillName + " CRITICAL HIT!");
            //    critDamage = 200f + sourceCharacter.critDamage;
            //}
            //BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
            //if (chosenBodyPart == null) {
            //    Debug.LogError("NO MORE BODY PARTS!");
            //    return;
            //}
            ////Armor armor = chosenBodyPart.GetArmor();
            //log += sourceCharacter.coloredUrlName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " in the " + chosenBodyPart.name.ToLower();

            //if (weapon != null) {
            //    damageRange = ItemManager.Instance.weaponTypeData[weapon.weaponType].damageRange;

            //    log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " " + weapon.itemName + ".";

            //} else {
            //    log += ".";
            //}

            //int finalAttack = 0;
            //if (attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL) {
            //    finalAttack = sourceCharacter.pFinalAttack;
            //} else {
            //    finalAttack = sourceCharacter.mFinalAttack;
            //}
            //int damage = (int) (((float) finalAttack * (attackSkill.power / 100f)) * (critDamage / 100f));
            //int computedDamageRange = (int) ((float) damage * (damageRange / 100f));
            //int minDamageRange = damage - computedDamageRange;
            //int maxDamageRange = damage + computedDamageRange;
            //damage = Utilities.rng.Next((minDamageRange < 0 ? 0 : minDamageRange), maxDamageRange + 1);

            ////Reduce damage by defense of target
            //damage -= def;

            //TODO: Add final damage bonus

            //Calculate elemental weakness and resistance
            //Use element of skill if it has one, if not, use weapon element instead if it has one
            //ELEMENT elementUsed = ELEMENT.NONE;
            //if (attackSkill.element != ELEMENT.NONE) {
            //    elementUsed = attackSkill.element;
            //} else {
            //    if (weapon != null && weapon.element != ELEMENT.NONE) {
            //        elementUsed = weapon.element;
            //    }
            //}
            //float elementalWeakness = 0f;
            //float elementalResistance = 0f;
            //if (targetCharacter.elementalWeaknesses != null && targetCharacter.elementalWeaknesses.ContainsKey(elementUsed)) {
            //    elementalWeakness = targetCharacter.elementalWeaknesses[elementUsed];
            //}
            //if (targetCharacter.elementalResistances != null && targetCharacter.elementalResistances.ContainsKey(elementUsed)) {
            //    elementalResistance = targetCharacter.elementalResistances[elementUsed];
            //}
            //float elementalDiff = elementalWeakness - elementalResistance;
            //float elementModifier = 1f + ((elementalDiff < 0f ? 0f : elementalDiff) / 100f);
            ////Add elemental weakness and resist to sourceCharacter battle tracker
            //if (attacker != null) {
            //    if (elementalWeakness > 0f) {
            //        attacker.battleTracker.AddEnemyElementalWeakness(targetCharacter.name, elementUsed);
            //    }
            //    if (elementalResistance > 0f) {
            //        attacker.battleTracker.AddEnemyElementalResistance(targetCharacter.name, elementUsed);
            //    }
            //}

            //Calculate total damage
            //damage = (int) ((float) damage * elementModifier);
            //if (damage < 1) {
            //    damage = 1;
            //}
            //log += "(" + damage.ToString() + ")";

            //DealDamageToBodyPart(attackSkill, targetCharacter, sourceCharacter, chosenBodyPart, ref log);

            //AddCombatLog(log, sourceCharacter.currentSide);

            //int previousCurrentHP = targetCharacter.currentHP;
            //targetCharacter.AdjustHP(-damage, sourceCharacter);

            ////Add HP Lost
            //int lastDamageTaken = previousCurrentHP - targetCharacter.currentHP;
            //float hpLost = ((float) lastDamageTaken / (float) targetCharacter.maxHP) * 100f;
            //targetCharacter.battleOnlyTracker.hpLostPercent += hpLost;
            //targetCharacter.battleOnlyTracker.lastDamageTaken = lastDamageTaken;

            ////Add previous actual damage
            //if (attacker != null) {
            //    attacker.battleTracker.SetLastDamageDealt(targetCharacter.name, damage);
            //}
        }

        private bool IsCombatAttributeApplicable(CombatAttribute combatAttribute, ICharacter targetCharacter, AttackSkill skill) {
            if (combatAttribute.requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.CLASS) {
                if (targetCharacter.characterClass != null && targetCharacter.characterClass.className.ToLower() == combatAttribute.requirement.ToLower()) {
                    return true;
                }
            } else if (combatAttribute.requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.RACE) {
                if (targetCharacter.race.ToString().ToLower() == combatAttribute.requirement.ToLower()) {
                    return true;
                }
            } else if (combatAttribute.requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.ELEMENT) {
                if (skill.element.ToString().ToLower() == combatAttribute.requirement.ToLower()) {
                    return true;
                }
            } else if (combatAttribute.requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.ATTRIBUTE) {
                if (targetCharacter.GetAttribute(combatAttribute.requirement) != null) {
                    return true;
                }
            }
            return false;
        }
		private string StatusEffectLog(ICharacter sourceCharacter, ICharacter targetCharacter, STATUS_EFFECT statusEffect){
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
		#endregion

		#region Heal Skill
		private void HealSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
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
		private void FleeSkill(ICharacter sourceCharacter, ICharacter targetCharacter){
            //TODO: ICharacter flees
            if (RemoveCharacter(targetCharacter)) {
                fledCharacters.Add(targetCharacter);
                //targetCharacter.SetIsDefeated (true);
                if (targetCharacter.IsInOwnParty()) { // the fled character is in his own party
                    if (targetCharacter.ownParty is CharacterParty) {
                        CharacterParty party = targetCharacter.ownParty as CharacterParty;
                        if (party.actionData.isHalted) {
                            party.actionData.SetIsHalted(false);
                        }
                        party.DisbandPartyKeepOwner(); //leave the other party members
                        //CombatManager.Instance.PartyContinuesActionAfterCombat(targetCharacter.ownParty as CharacterParty, false);
                    }
                } else {
                    if (targetCharacter.ownParty is CharacterParty) {
                        CharacterParty party = targetCharacter.ownParty as CharacterParty;
                        if (party.actionData.isHalted) {
                            party.actionData.SetIsHalted(false);
                        }
                    }
                    if (targetCharacter.currentParty is CharacterParty) {
                        //the fled character is in another character's party, leave the party
                        targetCharacter.currentParty.RemoveCharacter(targetCharacter); //this will also trigger the end of the characters In Party action
                    }
                }
                AddCombatLog(targetCharacter.coloredUrlName + " chickened out and ran away!", targetCharacter.currentSide);
            }
		}
		#endregion

		#region Obtain Item Skill
		private void ObtainItemSkill(ICharacter sourceCharacter, ICharacter targetCharacter){
			//TODO: ICharacter obtains an item
			AddCombatLog(targetCharacter.coloredUrlName + " obtained an item.", targetCharacter.currentSide);
		}
		#endregion

		#region Move Skill
		private void MoveSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
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
		internal void CharacterDeath(ICharacter character, ICharacter killer){
			if(RemoveCharacter(character)) {
                //Give exp to other side if monster died
                if(killer != null) {
                    if (character.icharacterType == ICHARACTER_TYPE.MONSTER) {
                        Monster monster = character as Monster;
                        List<ICharacter> killerCharacters = GetCharactersOnSide(killer.currentSide);
                        for (int i = 0; i < killerCharacters.Count; i++) {
                            killerCharacters[i].AdjustExperience(monster.experienceDrop);
                        }

                        //Give item drops to killer
                        if (killer is Character) {
                            Character killerChar = killer as Character;
                            int chance = 0;
                            foreach (KeyValuePair<string, float> itemDrop in monster.itemDropsLookup) {
                                chance = Utilities.rng.Next(0, 100);
                                if (chance < itemDrop.Value) {
                                    Item item = ItemManager.Instance.allItems[itemDrop.Key].CreateNewCopy();
                                    killerChar.PickupItem(item);
                                }
                            }
                        }

                    }
                }
                //deadCharacters.Add (character);
                //character.SetIsDefeated (true);
                AddCombatLog(character.coloredUrlName + " died horribly!", character.currentSide);
            }
		}
		internal void CharacterFainted(ICharacter character){
            if (RemoveCharacter(character)) {
                faintedCharacters.Add(character);
                //character.SetIsDefeated (true);
                AddCombatLog(character.coloredUrlName + " fainted!", character.currentSide);
            }
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

		public ICharacter GetAliveCharacterByID(int id, string type){
			for (int i = 0; i < this.characterSideACopy.Count; i++) {
                if(type == "character") {
                    if (!this.characterSideACopy[i].isDead && this.characterSideACopy[i].id == id && this.characterSideACopy[i].icharacterType == ICHARACTER_TYPE.CHARACTER) {
                        return this.characterSideACopy[i];
                    }
                } else if (type == "monster") {
                    if (!this.characterSideACopy[i].isDead && this.characterSideACopy[i].id == id && this.characterSideACopy[i].icharacterType == ICHARACTER_TYPE.MONSTER) {
                        return this.characterSideACopy[i];
                    }
                }
				
			}
			for (int i = 0; i < this.characterSideBCopy.Count; i++) {
                if (type == "character") {
                    if (!this.characterSideBCopy[i].isDead && this.characterSideBCopy[i].id == id && this.characterSideBCopy[i].icharacterType == ICHARACTER_TYPE.CHARACTER) {
                        return this.characterSideBCopy[i];
                    }
                } else if (type == "monster") {
                    if (!this.characterSideBCopy[i].isDead && this.characterSideBCopy[i].id == id && this.characterSideBCopy[i].icharacterType == ICHARACTER_TYPE.MONSTER) {
                        return this.characterSideBCopy[i];
                    }
                }
            }
			return null;
		}

        public void AddAfterCombatAction(Action action) {
            afterCombatActions.Add(action);
        }
    }
}

