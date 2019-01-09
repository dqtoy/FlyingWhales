using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            character.actRate = 0;
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
        int rounds = 1;
        while (this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0) {
            Debug.Log("========== Round " + rounds.ToString() + " ==========");
            ICharacterSim characterThatWillAct = GetCharacterThatWillAct(this.charactersSideA, this.charactersSideB);
            if (characterThatWillAct != null) {
                CharacterSim actingCharacter = null;
                if (characterThatWillAct.icharacterType == ICHARACTER_TYPE.CHARACTER) {
                    actingCharacter = characterThatWillAct as CharacterSim;
                }
                Debug.Log((actingCharacter != null ? actingCharacter.characterClass.className : "") + characterThatWillAct.name + " will act.");

                //Debug.Log((targetCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER ? (targetCharacter as CharacterSim).characterClass.className : "") + targetCharacter.name + " is the target. (hp lost: " + targetCharacter.battleOnlyTracker.hpLostPercent
                //        + ", last damage taken: " + targetCharacter.battleOnlyTracker.lastDamageTaken);

                //characterThatWillAct.EnableDisableSkills(this);
                //Debug.Log("Available Skills: ");
                //for (int i = 0; i < characterThatWillAct.skills.Count; i++) {
                //    Skill currSkill = characterThatWillAct.skills[i];
                //    if (currSkill.isEnabled) {
                //        Debug.Log(currSkill.skillName);
                //    }
                //}
                Skill skillToUse = GetSkillToUse(characterThatWillAct);
                if (skillToUse != null) {
                    List<ICharacterSim> targetCharacter = GetTargetCharacter(characterThatWillAct, skillToUse);
                    Debug.Log(characterThatWillAct.name + " decides to use " + skillToUse.skillName);
                    if(targetCharacter.Count > 1) {
                        Debug.Log(characterThatWillAct.name + " decides to use it on all enemies");
                    } else {
                        Debug.Log(characterThatWillAct.name + " decides to use it on " + targetCharacter[0].name);
                    }
                    DoSkill(skillToUse, characterThatWillAct, targetCharacter);
                }
            }
            Debug.Log("========== End Round " + rounds.ToString() + " ==========");
            rounds++;
            yield return new WaitForSeconds(0.05f);
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
            //AddCombatLog("  " + character.idName + " (" + character.currentHP + "/" + character.maxHP + ")", SIDES.A);
        }
    }

    private ICharacterSim GetCharacterThatWillAct(List<ICharacterSim> charactersSideA, List<ICharacterSim> charactersSideB) {
        List<ICharacterSim> candidates = new List<ICharacterSim>();
        for (int i = 0; i < charactersSideA.Count; i++) {
            charactersSideA[i].actRate += charactersSideA[i].speed;
            if (charactersSideA[i].actRate >= 1000f) {
                candidates.Add(charactersSideA[i]);
            }
        }
        for (int i = 0; i < charactersSideB.Count; i++) {
            charactersSideB[i].actRate += charactersSideB[i].speed;
            if (charactersSideB[i].actRate >= 1000f) {
                candidates.Add(charactersSideB[i]);
            }
        }
        if (candidates.Count > 0) {
            ICharacterSim chosenCharacter = null;
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
    private List<ICharacterSim> GetTargetCharacter(ICharacterSim sourceCharacter, Skill skill) {
        List<ICharacterSim> targets = null;
        if(skill.skillType == SKILL_TYPE.ATTACK) {
            if (sourceCharacter.currentSide == SIDES.B) {
                targets = this.charactersSideA;
            } else {
                targets = this.charactersSideB;
            }
        }else if (skill.skillType == SKILL_TYPE.HEAL) {
            if (sourceCharacter.currentSide == SIDES.B) {
                targets = this.charactersSideB;
            } else {
                targets = this.charactersSideA;
            }
        }

        if (skill.targetType == TARGET_TYPE.PARTY) {
            return targets;
        }
        return new List<ICharacterSim>() { targets[Utilities.rng.Next(0, targets.Count)] };
    }
    //public bool HasTargetInRangeForSkill(Skill skill, ICharacterSim sourceCharacter) {
    //    if (skill is AttackSkill) {
    //        if (sourceCharacter.currentSide == SIDES.A) {
    //            for (int i = 0; i < this.charactersSideB.Count; i++) {
    //                ICharacterSim targetCharacter = this.charactersSideB[i];
    //                int rowDistance = GetRowDistanceBetweenTwoCharacters(sourceCharacter, targetCharacter);
    //                if (skill.range >= rowDistance) {
    //                    return true;
    //                }
    //            }
    //        } else {
    //            for (int i = 0; i < this.charactersSideA.Count; i++) {
    //                ICharacterSim targetCharacter = this.charactersSideA[i];
    //                int rowDistance = GetRowDistanceBetweenTwoCharacters(sourceCharacter, targetCharacter);
    //                if (skill.range >= rowDistance) {
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    } else {
    //        return true;
    //    }

    //}
    //public bool HasTargetInRangeForSkill(SKILL_TYPE skillType, ICharacterSim sourceCharacter) {
    //    if (skillType == SKILL_TYPE.ATTACK) {
    //        for (int i = 0; i < sourceCharacter.skills.Count; i++) {
    //            Skill skill = sourceCharacter.skills[i];
    //            if (skill is AttackSkill) {
    //                return HasTargetInRangeForSkill(skill, sourceCharacter);
    //            }
    //        }
    //    }
    //    return true;
    //}
    private int GetRowDistanceBetweenTwoCharacters(ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
        if (distance < 0) {
            distance *= -1;
        }
        return distance;
    }
    private Skill GetSkillToUse(ICharacterSim sourceCharacter, ICharacterSim targetCharacter = null) {
        Debug.Log("Available Skills: " + sourceCharacter.skills[0].skillName);
        return sourceCharacter.skills[0];
    }
    private void DoSkill(Skill skill, ICharacterSim sourceCharacter, List<ICharacterSim> targetCharacter) {
        //If skill is attack, reduce sp
        //if (skill.skillType == SKILL_TYPE.ATTACK) {
        //    AttackSkill attackSkill = skill as AttackSkill;
        //    sourceCharacter.AdjustSP(-attackSkill.spCost);
        //}
        SuccessfulSkill(skill, sourceCharacter, targetCharacter);
    }

    //Go here if skill is accurate and is successful
    private void SuccessfulSkill(Skill skill, ICharacterSim sourceCharacter, List<ICharacterSim> targetCharacter) {
        AttackSkill(skill, sourceCharacter, targetCharacter);
        //if (skill is AttackSkill) {
        //    AttackSkill(skill, sourceCharacter, targetCharacter);
        //} else if (skill is HealSkill) {
        //    HealSkill(skill, sourceCharacter, targetCharacter);
        //} else if (skill is FleeSkill) {
        //    targetCharacter = sourceCharacter;
        //    FleeSkill(sourceCharacter, targetCharacter);
        //} else if (skill is ObtainSkill) {
        //    ObtainItemSkill(sourceCharacter, targetCharacter);
        //} else if (skill is MoveSkill) {
        //    targetCharacter = sourceCharacter;
        //    MoveSkill(skill, sourceCharacter, targetCharacter);
        //}
    }

    #region Attack Skill
    private void AttackSkill(Skill skill, ICharacterSim sourceCharacter, List<ICharacterSim> targetCharacter) {
        //AttackSkill attackSkill = skill as AttackSkill;
        HitTargetCharacter(skill, sourceCharacter, targetCharacter);
    }

    //Hits the target with an attack skill
    private void HitTargetCharacter(Skill attackSkill, ICharacterSim sourceCharacter, List<ICharacterSim> targetCharacters) {
        //for (int j = 0; j < targetCharacters.Count; j++) {
        //    ICharacterSim targetCharacter = targetCharacters[j];
        //    string log = string.Empty;
        //    float attackPower = sourceCharacter.attackPower;
        //    if (sourceCharacter.combatAttributes != null) {
        //        //Apply all flat damage attack power modifier first
        //        for (int i = 0; i < sourceCharacter.combatAttributes.Count; i++) {
        //            if (!sourceCharacter.combatAttributes[i].isPercentage && sourceCharacter.combatAttributes[i].stat == STAT.ATTACK && sourceCharacter.combatAttributes[i].hasRequirement
        //                && sourceCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.DEALT) {
        //                if(attackSkill.skillType == SKILL_TYPE.HEAL && sourceCharacter.combatAttributes[i].requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //                    continue;
        //                }
        //                if (IsCombatAttributeApplicable(sourceCharacter.combatAttributes[i], targetCharacter, attackSkill)) {
        //                    attackPower += sourceCharacter.combatAttributes[i].amount;
        //                }
        //            }
        //        }
        //        for (int i = 0; i < targetCharacter.combatAttributes.Count; i++) {
        //            if (!targetCharacter.combatAttributes[i].isPercentage && targetCharacter.combatAttributes[i].stat == STAT.ATTACK && targetCharacter.combatAttributes[i].hasRequirement
        //                && targetCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.RECEIVED) {
        //                if (attackSkill.skillType == SKILL_TYPE.HEAL && sourceCharacter.combatAttributes[i].requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //                    continue;
        //                }
        //                if (IsCombatAttributeApplicable(targetCharacter.combatAttributes[i], sourceCharacter, attackSkill)) {
        //                    attackPower += targetCharacter.combatAttributes[i].amount;
        //                }
        //            }
        //        }

        //        //Then apply all percentage modifiers
        //        for (int i = 0; i < sourceCharacter.combatAttributes.Count; i++) {
        //            if (sourceCharacter.combatAttributes[i].isPercentage && sourceCharacter.combatAttributes[i].stat == STAT.ATTACK && sourceCharacter.combatAttributes[i].hasRequirement
        //                && sourceCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.DEALT) {
        //                if (attackSkill.skillType == SKILL_TYPE.HEAL && sourceCharacter.combatAttributes[i].requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //                    continue;
        //                }
        //                if (IsCombatAttributeApplicable(sourceCharacter.combatAttributes[i], targetCharacter, attackSkill)) {
        //                    float result = attackPower * (sourceCharacter.combatAttributes[i].amount / 100f);
        //                    attackPower += result;
        //                }
        //            }
        //        }
        //        for (int i = 0; i < targetCharacter.combatAttributes.Count; i++) {
        //            if (targetCharacter.combatAttributes[i].isPercentage && targetCharacter.combatAttributes[i].stat == STAT.ATTACK && targetCharacter.combatAttributes[i].hasRequirement
        //                && targetCharacter.combatAttributes[i].damageIdentifier == DAMAGE_IDENTIFIER.RECEIVED) {
        //                if (attackSkill.skillType == SKILL_TYPE.HEAL && sourceCharacter.combatAttributes[i].requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //                    continue;
        //                }
        //                if (IsCombatAttributeApplicable(targetCharacter.combatAttributes[i], sourceCharacter, attackSkill)) {
        //                    float result = attackPower * (targetCharacter.combatAttributes[i].amount / 100f);
        //                    attackPower += result;
        //                }
        //            }
        //        }
        //    }
        //    int damage = (int) attackPower;
        //    log += sourceCharacter.idName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.idName + "(" + damage.ToString() + ")"; ;
        //    AddCombatLog(log, sourceCharacter.currentSide);

        //    if(attackSkill.skillType == SKILL_TYPE.ATTACK) {
        //        targetCharacter.AdjustHP(-damage);
        //    } else if (attackSkill.skillType == SKILL_TYPE.HEAL) {
        //        targetCharacter.AdjustHP(damage);
        //    }
        //}
       

        ////Reset attack miss
        //sourceCharacter.battleOnlyTracker.ResetAttackMiss(attackSkill.skillName);

        //string log = string.Empty;
        //CharacterSim attacker = null;
        //float damageRange = 0f;
        //Weapon weapon = null;
        //int statMod = sourceCharacter.strength;
        //int def = targetCharacter.GetDef();
        //float critDamage = 100f;
        //if (sourceCharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
        //    attacker = sourceCharacter as CharacterSim;
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
        //log += sourceCharacter.idName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.idName + " in the " + chosenBodyPart.name.ToLower();

        //if (weapon != null) {
        //    damageRange = CombatSimManager.Instance.weaponTypeData[weapon.weaponType].damageRange;

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

        ////TODO: Add final damage bonus

        ////Calculate elemental weakness and resistance
        ////Use element of skill if it has one, if not, use weapon element instead if it has one
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

        ////Calculate total damage
        //damage = (int) ((float) damage * elementModifier);
        //if(damage < 1) {
        //    damage = 1;
        //}
        //log += "(" + damage.ToString() + ")";

        //AddCombatLog(log, sourceCharacter.currentSide);

        //int previousCurrentHP = targetCharacter.currentHP;
        //targetCharacter.AdjustHP(-damage);

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
    private bool IsCombatAttributeApplicable(Trait combatAttribute, ICharacterSim targetCharacter, Skill skill) {
        //if (combatAttribute.requirementType == TRAIT_REQUIREMENT.CLASS) {
        //    if (targetCharacter.characterClass != null && targetCharacter.characterClass.className.ToLower() == combatAttribute.requirement.ToLower()) {
        //        return true;
        //    }
        //} else if (combatAttribute.requirementType == TRAIT_REQUIREMENT.RACE) {
        //    if (targetCharacter.race.ToString().ToLower() == combatAttribute.requirement.ToLower()) {
        //        return true;
        //    }
        //} else if (combatAttribute.requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //    if (skill.element.ToString().ToLower() == combatAttribute.requirement.ToLower()) {
        //        return true;
        //    }
        //} else if (combatAttribute.requirementType == TRAIT_REQUIREMENT.ATTRIBUTE) {
        //    if (targetCharacter.GetAttribute(combatAttribute.requirement) != null) {
        //        return true;
        //    }
        //}
        return false;
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
