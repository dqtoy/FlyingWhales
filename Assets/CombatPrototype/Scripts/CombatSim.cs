using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.Linq;

public class CombatSim {

    public List<ICharacterSim> charactersSideA;
    public List<ICharacterSim> charactersSideB;
    public List<string> resultsLog;

    public CombatSim() {
        charactersSideA = new List<ICharacterSim>();
        charactersSideB = new List<ICharacterSim>();
        resultsLog = new List<string>();
    }


    public void AddCharacter(SIDES side, ICharacterSim character) {
        if (!this.charactersSideA.Contains(character) && !this.charactersSideB.Contains(character)) {
            int rowNumber = 1;
            if (side == SIDES.A) {
                this.charactersSideA.Add(character);
            } else {
                this.charactersSideB.Add(character);
                rowNumber = 5;
            }
            character.SetSide(side);
            //character.currentCombat = this;
            character.SetRowNumber(rowNumber);
            character.actRate = character.speed;
        }
    }
    public bool RemoveCharacter(ICharacterSim character) {
        if (this.charactersSideA.Remove(character)) {
            return true;
        } else if (this.charactersSideB.Remove(character)) {
            return true;
        }
        return false;
    }

    public void CharacterDeath(ICharacterSim character) {
        if (RemoveCharacter(character)) {
            AddCombatLog(character.idName + " died horribly!", character.currentSide);
        }
    }
    //This simulates the whole combat system
    public void CombatSimulation() {
        CombatSimManager.Instance.StartCoroutine(CombatSimulationCoroutine());
    }

    private IEnumerator CombatSimulationCoroutine() {
        ClearCombatLogs();
        AddCombatLog("Combat starts", SIDES.A);
        Dictionary<ICharacterSim, int> characterActivationWeights = new Dictionary<ICharacterSim, int>();

        int rounds = 1;
        while (this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0) {
            Debug.Log("========== Round " + rounds.ToString() + " ==========");
            ICharacterSim characterThatWillAct = GetCharacterThatWillAct(characterActivationWeights, this.charactersSideA, this.charactersSideB);
            ICharacterSim targetCharacter = GetTargetCharacter(characterThatWillAct, null);

            CharacterSim actingCharacter = null;
            if (characterThatWillAct.icharacterType == ICHARACTER_TYPE.CHARACTER) {
                actingCharacter = characterThatWillAct as CharacterSim;
            }
            Debug.Log((actingCharacter != null ? actingCharacter.characterClass.className : "") + characterThatWillAct.name + " will act. (hp lost: " + characterThatWillAct.battleOnlyTracker.hpLostPercent
                    + ", last damage taken: " + characterThatWillAct.battleOnlyTracker.lastDamageTaken);
            Debug.Log((targetCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER ? (targetCharacter as CharacterSim).characterClass.className : "") + targetCharacter.name + " is the target. (hp lost: " + targetCharacter.battleOnlyTracker.hpLostPercent
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
                //ICharacter targetCharacter = GetTargetCharacter(characterThatWillAct, skillToUse);
                Debug.Log(characterThatWillAct.name + " decides to use it on " + targetCharacter.name);
                DoSkill(skillToUse, characterThatWillAct, targetCharacter);
            }
            Debug.Log("========== End Round " + rounds.ToString() + " ==========");
            rounds++;
            yield return new WaitForSeconds(0.5f);
        }
        SIDES winner = SIDES.A;
        List<ICharacterSim> winnerCharacters = CombatSimManager.Instance.sideAList;
        if(charactersSideB.Count > 0) {
            winner = SIDES.B;
            winnerCharacters = CombatSimManager.Instance.sideBList;
        }
        AddCombatLog("Combat Ends! Winning Side: " + winner.ToString(), SIDES.A);
        AddCombatLog("Winners:", SIDES.A);
        for (int i = 0; i < winnerCharacters.Count; i++) {
            ICharacterSim character = winnerCharacters[i];
            AddCombatLog("  " + character.idName + " (" + character.currentHP + "/" + character.maxHP + ")", SIDES.A);
        }
    }

    private ICharacterSim GetCharacterThatWillAct(Dictionary<ICharacterSim, int> characterActivationWeights, List<ICharacterSim> charactersSideA, List<ICharacterSim> charactersSideB) {
        characterActivationWeights.Clear();
        for (int i = 0; i < charactersSideA.Count; i++) {
            characterActivationWeights.Add(charactersSideA[i], charactersSideA[i].actRate);
        }
        for (int i = 0; i < charactersSideB.Count; i++) {
            characterActivationWeights.Add(charactersSideB[i], charactersSideB[i].actRate);
        }

        ICharacterSim chosenCharacter = Utilities.PickRandomElementWithWeights<ICharacterSim>(characterActivationWeights);
        foreach (ICharacterSim character in characterActivationWeights.Keys) {
            character.actRate += character.speed;
        }
        chosenCharacter.actRate = chosenCharacter.speed;
        return chosenCharacter;
    }
    private ICharacterSim GetTargetCharacter(ICharacterSim sourceCharacter, Skill skill) {
        List<ICharacterSim> oppositeTargets = this.charactersSideB;
        if (sourceCharacter.currentSide == SIDES.B) {
            oppositeTargets = this.charactersSideA;
        }
        return oppositeTargets[Utilities.rng.Next(0, oppositeTargets.Count)];
    }
    public bool HasTargetInRangeForSkill(Skill skill, ICharacterSim sourceCharacter) {
        if (skill is AttackSkill) {
            if (sourceCharacter.currentSide == SIDES.A) {
                for (int i = 0; i < this.charactersSideB.Count; i++) {
                    ICharacterSim targetCharacter = this.charactersSideB[i];
                    int rowDistance = GetRowDistanceBetweenTwoCharacters(sourceCharacter, targetCharacter);
                    if (skill.range >= rowDistance) {
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < this.charactersSideA.Count; i++) {
                    ICharacterSim targetCharacter = this.charactersSideA[i];
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
    public bool HasTargetInRangeForSkill(SKILL_TYPE skillType, ICharacterSim sourceCharacter) {
        if (skillType == SKILL_TYPE.ATTACK) {
            for (int i = 0; i < sourceCharacter.skills.Count; i++) {
                Skill skill = sourceCharacter.skills[i];
                if (skill is AttackSkill) {
                    return HasTargetInRangeForSkill(skill, sourceCharacter);
                }
            }
        }
        return true;
    }
    private int GetRowDistanceBetweenTwoCharacters(ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
        if (distance < 0) {
            distance *= -1;
        }
        return distance;
    }
    private Skill GetSkillToUse(ICharacterSim sourceCharacter, ICharacterSim targetCharacter = null) {
        Debug.Log("Available Skills: ");
        Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int>();
        for (int i = 0; i < sourceCharacter.skills.Count; i++) { //These are general skills like move, flee, drink potion
            Skill skill = sourceCharacter.skills[i];
            if (skill.isEnabled && skill.skillType != SKILL_TYPE.ATTACK) {
                Debug.Log(skill.skillName);
                skillActivationWeights.Add(skill, skill.activationWeight);
            }
        }


        float weaponAttack = 0f;
        float missingHP = (1f - ((float) sourceCharacter.currentHP / (float) sourceCharacter.maxHP)) * 100f;
        int levelDiff = (sourceCharacter.level - targetCharacter.level) * 10;
        if (sourceCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            CharacterSim character = sourceCharacter as CharacterSim;
            if (sourceCharacter.battleOnlyTracker.lastDamageTaken < sourceCharacter.currentHP) {//character must have a weapon and sourceCharacter last damage taken must not be >= current health
                weaponAttack = character.weaponAttack;
                for (int i = 0; i < character.skills.Count; i++) {
                    Skill skill = character.skills[i];
                    if (skill.isEnabled && skill.skillType == SKILL_TYPE.ATTACK) {
                        Debug.Log(skill.skillName);
                        AttackSkill attackSkill = skill as AttackSkill;
                        float initialWeight = GetSkillInitialWeight(sourceCharacter, targetCharacter, attackSkill, weaponAttack, missingHP, levelDiff, character.battleTracker);
                        float specialModifier = GetSpecialModifier(sourceCharacter, targetCharacter, attackSkill, character.battleTracker);
                        int finalWeight = Mathf.CeilToInt(initialWeight * (specialModifier / 100f));
                        if (finalWeight > 0) {
                            skillActivationWeights.Add(attackSkill, finalWeight);
                        }
                    }
                }
            }
        } else if (sourceCharacter.icharacterType == ICHARACTER_TYPE.MONSTER) {
            Monster monster = sourceCharacter as Monster;
            weaponAttack = monster.attackPower;
            for (int i = 0; i < monster.skills.Count; i++) {
                Skill skill = monster.skills[i];
                if (skill.isEnabled && skill.skillType == SKILL_TYPE.ATTACK) {
                    Debug.Log(skill.skillName);
                    AttackSkill attackSkill = skill as AttackSkill;
                    float initialWeight = (float) attackSkill.activationWeight; // GetSkillInitialWeight(sourceCharacter, targetCharacter, attackSkill, weaponAttack, missingHP, levelDiff);
                    float specialModifier = GetSpecialModifier(sourceCharacter, targetCharacter, attackSkill);
                    int finalWeight = Mathf.CeilToInt(initialWeight * (specialModifier / 100f));
                    if (finalWeight > 0) {
                        skillActivationWeights.Add(attackSkill, finalWeight);
                    }
                }
            }
        }

        if (skillActivationWeights.Count > 0) {
            Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill>(skillActivationWeights);
            return chosenSkill;
        }
        return null;
    }

    private float GetSkillInitialWeight(ICharacterSim sourceCharacter, ICharacterSim targetCharacter, AttackSkill attackSkill, float weaponAttack, float missingHP, int levelDiff, CharacterBattleTracker battleTracker = null) {
        //int statUsed = attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL ? sourceCharacter.strength : sourceCharacter.intelligence;
        int finalAttack = 0;
        if (attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL) {
            finalAttack = sourceCharacter.pFinalAttack;
        } else {
            finalAttack = sourceCharacter.mFinalAttack;
        }
        float rawDamage = ((float) finalAttack * ((float) attackSkill.power / 100f)) * 1; //Subject to change: the *1 is the targets that will hit, this will change in the future

        float modifier = GetModifier(sourceCharacter, targetCharacter, attackSkill, rawDamage, missingHP, levelDiff, battleTracker);
        float spModifier = sourceCharacter.currentSP - attackSkill.spCost;
        if (spModifier <= 0f) { spModifier = 1f; }
        return (rawDamage * (1f + (modifier / 100f))) / spModifier;
    }
    private float GetModifier(ICharacterSim sourceCharacter, ICharacterSim targetCharacter, AttackSkill attackSkill, float rawDamage, float missingHP, int levelDiff, CharacterBattleTracker battleTracker = null) {
        //Elemental Weakness
        float elementalWeakness = 0f;
        if (attackSkill.element != ELEMENT.NONE && battleTracker != null) {
            if (battleTracker.tracker.ContainsKey(targetCharacter.name)) {
                if (battleTracker.tracker[targetCharacter.name].elementalWeaknesses.Contains(attackSkill.element)) {
                    elementalWeakness = targetCharacter.elementalWeaknesses[attackSkill.element];
                }
            }
        }

        //Missing HP from parameter
        //Level Difference from parameter

        //HP Lost
        float hpLostPercent = sourceCharacter.battleOnlyTracker.hpLostPercent;

        //Expected Value
        float previousActualDamage = battleTracker != null && battleTracker.tracker.ContainsKey(targetCharacter.name) ? battleTracker.tracker[targetCharacter.name].lastDamageDealt : 0f;
        float expectedValue = previousActualDamage / (rawDamage + previousActualDamage);

        return elementalWeakness + missingHP + levelDiff + hpLostPercent + expectedValue;
    }
    private float GetSpecialModifier(ICharacterSim sourceCharacter, ICharacterSim targetCharacter, AttackSkill attackSkill, CharacterBattleTracker battleTracker = null) {
        //Attack misses per skill
        float attackMissPercent = (-33.4f) * (float) (sourceCharacter.battleOnlyTracker.consecutiveAttackMisses.ContainsKey(attackSkill.skillName) ? sourceCharacter.battleOnlyTracker.consecutiveAttackMisses[attackSkill.skillName] : 0);

        //Elemental Resist
        float resistance = 0f;
        if (attackSkill.element != ELEMENT.NONE && battleTracker != null) {
            if (battleTracker.tracker.ContainsKey(targetCharacter.name)) {
                if (battleTracker.tracker[targetCharacter.name].elementalResistances.Contains(attackSkill.element)) {
                    resistance = targetCharacter.elementalResistances[attackSkill.element];
                }
            }
        }
        float elementalResistance = (100f - resistance) / 100f;

        //Will die next hit
        float dieNextHit = sourceCharacter.currentHP - sourceCharacter.battleOnlyTracker.lastDamageTaken;

        return attackMissPercent + resistance + elementalResistance + dieNextHit;
    }
    private void DoSkill(Skill skill, ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        //If skill is attack, reduce sp
        if (skill.skillType == SKILL_TYPE.ATTACK) {
            AttackSkill attackSkill = skill as AttackSkill;
            sourceCharacter.AdjustSP(-attackSkill.spCost);
        }
        SuccessfulSkill(skill, sourceCharacter, targetCharacter);
    }

    //Go here if skill is accurate and is successful
    private void SuccessfulSkill(Skill skill, ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
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

    #region Attack Skill
    private void AttackSkill(Skill skill, ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        AttackSkill attackSkill = skill as AttackSkill;
        HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter);
    }

    //Hits the target with an attack skill
    private void HitTargetCharacter(AttackSkill attackSkill, ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        //Reset attack miss
        sourceCharacter.battleOnlyTracker.ResetAttackMiss(attackSkill.skillName);

        string log = string.Empty;
        CharacterSim attacker = null;
        float damageRange = 0f;
        int statMod = sourceCharacter.strength;
        int def = targetCharacter.GetPDef(sourceCharacter);
        float critDamage = 100f;
        if (sourceCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            attacker = sourceCharacter as CharacterSim;
        }
        if (attackSkill.attackCategory == ATTACK_CATEGORY.MAGICAL) {
            statMod = sourceCharacter.intelligence;
            def = targetCharacter.GetMDef(sourceCharacter);
        }
        int critChance = Utilities.rng.Next(0, 100);
        if (critChance < sourceCharacter.critChance) {
            //CRITICAL HIT!
            Debug.Log(attackSkill.skillName + " CRITICAL HIT!");
            critDamage = 200f + sourceCharacter.critDamage;
        }
        BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
        if (chosenBodyPart == null) {
            Debug.LogError("NO MORE BODY PARTS!");
            return;
        }
        Armor armor = chosenBodyPart.GetArmor();
        log += sourceCharacter.idName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.idName + " in the " + chosenBodyPart.name.ToLower();

        if (attacker != null) {
            //damageRange = ItemManager.Instance.weaponTypeData[weapon.weaponType].damageRange;

            log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " weapon.";

        } else {
            log += ".";
        }

        int finalAttack = 0;
        if (attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL) {
            finalAttack = sourceCharacter.pFinalAttack;
        } else {
            finalAttack = sourceCharacter.mFinalAttack;
        }
        int damage = (int) (((float) finalAttack * (attackSkill.power / 100f)) * (critDamage / 100f));
        int computedDamageRange = (int) ((float) damage * (damageRange / 100f));
        int minDamageRange = damage - computedDamageRange;
        int maxDamageRange = damage + computedDamageRange;
        damage = Utilities.rng.Next((minDamageRange < 0 ? 0 : minDamageRange), maxDamageRange + 1);

        //Reduce damage by defense of target
        damage -= def;

        //Calculate elemental weakness and resistance
        //Use element of skill if it has one, if not, use weapon element instead if it has one
        ELEMENT elementUsed = ELEMENT.NONE;
        if (attackSkill.element != ELEMENT.NONE) {
            elementUsed = attackSkill.element;
        } else {
            //if (weapon != null && weapon.element != ELEMENT.NONE) {
            //    elementUsed = weapon.element;
            //}
        }
        float elementalWeakness = 0f;
        float elementalResistance = 0f;
        if (targetCharacter.elementalWeaknesses != null && targetCharacter.elementalWeaknesses.ContainsKey(elementUsed)) {
            elementalWeakness = targetCharacter.elementalWeaknesses[elementUsed];
        }
        if (targetCharacter.elementalResistances != null && targetCharacter.elementalResistances.ContainsKey(elementUsed)) {
            elementalResistance = targetCharacter.elementalResistances[elementUsed];
        }
        float elementalDiff = elementalWeakness - elementalResistance;
        float elementModifier = 1f + ((elementalDiff < 0f ? 0f : elementalDiff) / 100f);
        //Add elemental weakness and resist to sourceCharacter battle tracker
        if (attacker != null) {
            if (elementalWeakness > 0f) {
                attacker.battleTracker.AddEnemyElementalWeakness(targetCharacter.name, elementUsed);
            }
            if (elementalResistance > 0f) {
                attacker.battleTracker.AddEnemyElementalResistance(targetCharacter.name, elementUsed);
            }
        }

        //Calculate total damage
        damage = (int) ((float) damage * elementModifier);
        if(damage < 1) {
            damage = 1;
        }
        log += "(" + damage.ToString() + ")";

        AddCombatLog(log, sourceCharacter.currentSide);

        int previousCurrentHP = targetCharacter.currentHP;
        targetCharacter.AdjustHP(-damage);

        //Add HP Lost
        int lastDamageTaken = previousCurrentHP - targetCharacter.currentHP;
        float hpLost = ((float) lastDamageTaken / (float) targetCharacter.maxHP) * 100f;
        targetCharacter.battleOnlyTracker.hpLostPercent += hpLost;
        targetCharacter.battleOnlyTracker.lastDamageTaken = lastDamageTaken;

        //Add previous actual damage
        if (attacker != null) {
            attacker.battleTracker.SetLastDamageDealt(targetCharacter.name, damage);
        }
    }
    //Returns a random body part of a character
    private BodyPart GetRandomBodyPart(ICharacterSim character) {
        List<BodyPart> allBodyParts = character.bodyParts.Where(x => !x.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)).ToList();
        if (allBodyParts.Count > 0) {
            return allBodyParts[Utilities.rng.Next(0, allBodyParts.Count)];
        } else {
            return null;
        }
    }
    #endregion

    #region Heal Skill
    private void HealSkill(Skill skill, ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        HealSkill healSkill = (HealSkill) skill;
        targetCharacter.AdjustHP(healSkill.healPower);
        if (sourceCharacter == targetCharacter) {
            AddCombatLog(sourceCharacter.idName + " used " + healSkill.skillName + " and healed himself/herself for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
        } else if (sourceCharacter == targetCharacter) {
            AddCombatLog(sourceCharacter.idName + " used " + healSkill.skillName + " and healed " + targetCharacter.idName + " for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
        }

    }
    #endregion

    #region Flee Skill
    private void FleeSkill(ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        //TODO: ICharacter flees
        if (RemoveCharacter(targetCharacter)) {
            //fledCharacters.Add(targetCharacter);
            ////targetCharacter.SetIsDefeated (true);
            //if (targetCharacter.iparty is CharacterParty) {
            //    if (targetCharacter.iparty.icharacters.Count > 1) {
            //        targetCharacter.CreateNewParty();
            //    }
            //    CombatManager.Instance.PartyContinuesActionAfterCombat(targetCharacter.iparty as CharacterParty, false);
            //}
            AddCombatLog(targetCharacter.idName + " chickened out and ran away!", targetCharacter.currentSide);
        }
    }
    #endregion

    #region Obtain Item Skill
    private void ObtainItemSkill(ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        //TODO: ICharacter obtains an item
        AddCombatLog(targetCharacter.idName + " obtained an item.", targetCharacter.currentSide);
    }
    #endregion

    #region Move Skill
    private void MoveSkill(Skill skill, ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        if (skill.skillName == "MoveLeft") {
            if (targetCharacter.currentRow != 1) {
                targetCharacter.SetRowNumber(targetCharacter.currentRow - 1);
            }
            AddCombatLog(targetCharacter.idName + " moved to the left. (" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
        } else if (skill.skillName == "MoveRight") {
            if (targetCharacter.currentRow != 5) {
                targetCharacter.SetRowNumber(targetCharacter.currentRow + 1);
            }
            AddCombatLog(targetCharacter.idName + " moved to the right.(" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
        }
    }
    #endregion

    #region Logs
    public void AddCombatLog(string combatLog, SIDES side) {
        string newLog = combatLog;
        if(side == SIDES.B) {
            newLog = "<color=#FF0000>" + combatLog + "</color>";
        }
        //resultsLog.Add(combatLog);
        Debug.Log(combatLog);
        CombatSimManager.Instance.combatText.text += newLog + "\n";
    }
    public void ClearCombatLogs() {
        //resultsLog.Clear();
        CombatSimManager.Instance.combatText.text = string.Empty;
    }
    #endregion
}
