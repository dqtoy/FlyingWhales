using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Traits;

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
            AddCombatLog($"{character.idName} died horribly!", character.currentSide);
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
            Debug.Log($"========== Round {rounds} ==========");
            ICharacterSim characterThatWillAct = GetCharacterThatWillAct(this.charactersSideA, this.charactersSideB);
            if (characterThatWillAct != null) {
                CharacterSim actingCharacter = null;
                if (characterThatWillAct.icharacterType == ICHARACTER_TYPE.CHARACTER) {
                    actingCharacter = characterThatWillAct as CharacterSim;
                }
                Debug.Log(
                    $"{(actingCharacter != null ? actingCharacter.characterClass.className : "")}{characterThatWillAct.name} will act.");

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
            }
            Debug.Log($"========== End Round {rounds} ==========");
            rounds++;
            yield return new WaitForSeconds(0.05f);
        }
        SIDES winner = SIDES.A;
        List<ICharacterSim> winnerCharacters = CombatSimManager.Instance.sideAList;
        if(charactersSideB.Count > 0) {
            winner = SIDES.B;
            winnerCharacters = CombatSimManager.Instance.sideBList;
        }
        AddCombatLog($"Combat Ends! Winning Side: {winner}", SIDES.A);
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
            AddCombatLog($"{targetCharacter.idName} chickened out and ran away!", targetCharacter.currentSide);
        }
    }
    #endregion

    #region Obtain Item Skill
    private void ObtainItemSkill(ICharacterSim sourceCharacter, ICharacterSim targetCharacter) {
        //TODO: ICharacter obtains an item
        AddCombatLog($"{targetCharacter.idName} obtained an item.", targetCharacter.currentSide);
    }
    #endregion


    #region Logs
    public void AddCombatLog(string combatLog, SIDES side) {
        string newLog = combatLog;
        if(side == SIDES.B) {
            newLog = $"<color=#FF0000>{combatLog}</color>";
        }
        //resultsLog.Add(combatLog);
        Debug.Log(combatLog);
        CombatSimManager.Instance.combatText.text += $"{newLog}\n";
    }
    public void ClearCombatLogs() {
        //resultsLog.Clear();
        CombatSimManager.Instance.combatText.text = string.Empty;
    }
    #endregion
}
