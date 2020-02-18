using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Traits;

public class NewCombat : MonoBehaviour {
    public CombatGrid leftSide;
    public CombatGrid rightSide;

    private List<Character> _deadCharacters;
    private List<CombatCharacter> _combatOrder;
    private List<CombatCharacter> _leftSideCombatOrder;
    private List<CombatCharacter> _rightSideCombatOrder;
    private List<Action> _endCombatActions;

    public SIDES winningSide { get; private set; }
    public bool isSelectingTarget { get; private set; }
    public CombatCharacter currentAttacker { get; private set; }
    public List<CombatCharacter> selectedTargetCharacters { get; private set; }

    private bool _isPaused;

    #region getters/setters
    private bool isCombatEnd {
        get { return leftSide.IsGridEmpty() || rightSide.IsGridEmpty(); }
    }
    #endregion

    public void Initialize() {
        _deadCharacters = new List<Character>();
        _combatOrder = new List<CombatCharacter>();
        _leftSideCombatOrder = new List<CombatCharacter>();
        _rightSideCombatOrder = new List<CombatCharacter>();
        _endCombatActions = new List<Action>();
        leftSide = new CombatGrid();
        rightSide = new CombatGrid();
        leftSide.Initialize();
        rightSide.Initialize();
    }

    private void ResetCombat() {
        leftSide.ResetGrid();
        rightSide.ResetGrid();
        _deadCharacters.Clear();
        _combatOrder.Clear();
        _endCombatActions.Clear();
    }

    public void StartNewCombat() {
        ResetCombat();
    }
    public void AddCharacters(CombatGrid grid, SIDES side) {
        CombatGrid chosenGrid = leftSide;
        if(side == SIDES.B) {
            chosenGrid = rightSide;
        }
        for (int i = 0; i < grid.slots.Length; i++) {
            chosenGrid.slots[i].OccupySlot(grid.slots[i].character);
        }
    }
    public void AddCharacters(List<Character> characters, SIDES side) {
        CombatGrid chosenGrid = leftSide;
        if (side == SIDES.B) {
            chosenGrid = rightSide;
        }
        for (int i = 0; i < characters.Count; i++) {
            chosenGrid.AssignCharacterToGrid(characters[i]);
        }
    }
    public void AddEndCombatActions(Action action) {
        _endCombatActions.Add(action);
    }
    private void ReorderCombat() {
        for (int i = 0; i < _combatOrder.Count; i++) {
            _combatOrder[i].UpdateSpeed();
            for (int j = 0; j < i; j++) {
                if(_combatOrder[i].speed > _combatOrder[j].speed) {
                    CombatCharacter combatCharacter = _combatOrder[i];
                    _combatOrder.RemoveAt(i);
                    _combatOrder.Insert(j, combatCharacter);
                }else if (_combatOrder[i].speed == _combatOrder[j].speed) {
                    int chance = UnityEngine.Random.Range(0, 2);
                    int index = j;
                    if(chance == 0) {
                        //Insert on index + 1
                        index = j + 1;
                    }
                    if (index != i) {
                        CombatCharacter combatCharacter = _combatOrder[i];
                        _combatOrder.RemoveAt(i);
                        _combatOrder.Insert(index, combatCharacter);
                    }
                }
            }
        }
    }

    private void TransferSlotsToOrder() {
        for (int i = 0; i < leftSide.slots.Length; i++) {
            if (leftSide.slots[i].isOccupied && !IsInCombatOrder(leftSide.slots[i].character)) {
                CombatCharacter combatCharacter = new CombatCharacter(leftSide.slots[i].character, leftSide.slots[i].gridNumber, SIDES.A);
                _combatOrder.Add(combatCharacter);
                _leftSideCombatOrder.Add(combatCharacter);
                //leftSide.slots[i].character.SetCombatCharacter(combatCharacter);
                //leftSide.slots[i].character.AdjustDoNotDisturb(1);
            }
            if (rightSide.slots[i].isOccupied && !IsInCombatOrder(rightSide.slots[i].character)) {
                CombatCharacter combatCharacter = new CombatCharacter(rightSide.slots[i].character, rightSide.slots[i].gridNumber, SIDES.B);
                _combatOrder.Add(combatCharacter);
                _rightSideCombatOrder.Add(combatCharacter);
                //rightSide.slots[i].character.SetCombatCharacter(combatCharacter);
                //rightSide.slots[i].character.AdjustDoNotDisturb(1);
            }
        }
        ApplyStartCombatTraits();
    }
    private bool IsInCombatOrder(Character character) {
        for (int i = 0; i < _combatOrder.Count; i++) {
            if(_combatOrder[i].character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    public void SetPausedState(bool state) {
        if(_isPaused != state) {
            _isPaused = state;
        }
    }
    public void Fight() {
        StartCoroutine(StartCombatCoroutine());
    }
    private IEnumerator StartCombatCoroutine() {
        TransferSlotsToOrder();
        CombatCharacter previousCombatCharacter = null;
        while (!isCombatEnd) {
            ReorderCombat();
            for (int i = 0; i < _combatOrder.Count; i++) {
                if (previousCombatCharacter != null) {
                    UIManager.Instance.combatUI.UnhighlightAttacker(previousCombatCharacter.character, previousCombatCharacter.side);
                }
                CombatCharacter sourceCombatCharacter = _combatOrder[i];
                previousCombatCharacter = sourceCombatCharacter;
                UIManager.Instance.combatUI.HighlightAttacker(sourceCombatCharacter.character, sourceCombatCharacter.side);
                //Messenger.Broadcast(Signals.HIGHLIGHT_ATTACKER, sourceCombatCharacter.character, sourceCombatCharacter.side);
                UIManager.Instance.combatUI.AddCombatLogs($"{sourceCombatCharacter.character.name} will now attack!", sourceCombatCharacter.side);
                //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, sourceCombatCharacter.character.name + " will now attack!");

                //Get and Hit Targets
                //List<CombatCharacter> targetCharacters = null;
                //if (sourceCombatCharacter.character.minion != null) {
                //    SelectTarget(sourceCombatCharacter);
                //    yield return new WaitWhile(() => _isPaused == true);
                //    targetCharacters = selectedTargetCharacters;
                //} else {
                //    targetCharacters = GetTargetCharacters(sourceCombatCharacter);
                //}

                //Use Items, if any
                UseItem(sourceCombatCharacter);

                List<CombatCharacter> targetCharacters = GetTargetCharacters(sourceCombatCharacter);
                string attackLog = string.Empty;
                if (targetCharacters != null && targetCharacters.Count > 0) {
                    attackLog = $"{sourceCombatCharacter.character.name} attacks ";
                    for (int j = 0; j < targetCharacters.Count; j++) {
                        CombatCharacter target = targetCharacters[j];
                        if (j == 0) {
                            attackLog += target.character.name;
                        } else {
                            attackLog += $", {target.character.name}";
                        }
                        int finalAttack = GetFinalAttack(sourceCombatCharacter, target, targetCharacters);
                        target.character.AdjustHP(-finalAttack);
                        if (target.character.currentHP <= 0) {
                            int removedIndex = RemoveFromCombatOrder(target.character);
                            if(removedIndex != -1) {
                                AddToDeadCharacters(target.character);
                                if (removedIndex < i) {
                                    i--;
                                }
                            }
                        }
                        attackLog += $" for {finalAttack}";
                    }
                    attackLog += ".";
                    UIManager.Instance.combatUI.AddCombatLogs(attackLog, sourceCombatCharacter.side);
                    //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, attackLog);

                    string deathLog = string.Empty;
                    for (int j = 0; j < targetCharacters.Count; j++) {
                        if (targetCharacters[j].character.currentHP <= 0) {
                            if (deathLog == string.Empty) {
                                deathLog += targetCharacters[j].character.name;
                            } else {
                                deathLog += $", {targetCharacters[j].character.name}";
                            }
                        }
                    }
                    if (deathLog != string.Empty) {
                        deathLog += " is dead!";
                        UIManager.Instance.combatUI.AddCombatLogs(deathLog, sourceCombatCharacter.side);
                    }
                    //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, deathLog);
                } else {
                    attackLog =
                        $"No target for {sourceCombatCharacter.character.name}: {sourceCombatCharacter.character.characterClass.combatTarget}";
                    UIManager.Instance.combatUI.AddCombatLogs(attackLog, sourceCombatCharacter.side);
                    //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, attackLog);
                }

                //Messenger.Broadcast(Signals.UNHIGHLIGHT_ATTACKER, sourceCombatCharacter.character, sourceCombatCharacter.side);
                UIManager.Instance.combatUI.UpdateCombatSlotItems();
                //Messenger.Broadcast(Signals.UPDATE_COMBAT_GRIDS);
                yield return new WaitWhile(() => _isPaused == true);
                yield return new WaitForSeconds(0.75f);
                if (isCombatEnd) {
                    break;
                }
            }
        }
        if (rightSide.IsGridEmpty()) {
            winningSide = SIDES.A;
            UIManager.Instance.combatUI.AddCombatLogs("Left Side Wins!", SIDES.A);
            //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, "Left Side Wins!");
        } else {
            winningSide = SIDES.B;
            UIManager.Instance.combatUI.AddCombatLogs("Right Side Wins!", SIDES.B);
            //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, "Right Side Wins!");
        }
        //for (int i = 0; i < _combatOrder.Count; i++) {
        //    _combatOrder[i].character.AdjustDoNotDisturb(-1);
        //}
        //for (int i = 0; i < _deadCharacters.Count; i++) {
        //    _deadCharacters[i].AdjustDoNotDisturb(-1);
        //}
        for (int i = 0; i < _endCombatActions.Count; i++) {
            if (_endCombatActions[i] != null) {
                _endCombatActions[i]();
            }
        }
        //RemoveAllDeadFromCombatOrder();
    }
    private void RemoveAllDeadFromCombatOrder() {
        for (int i = 0; i < _combatOrder.Count; i++) {
            if (_combatOrder[i].character.currentHP <= 0) {
                _combatOrder.RemoveAt(i);
                i--;
            }
        }
    }
    private void AddToDeadCharacters(Character character) {
        if (!_deadCharacters.Contains(character)) {
            _deadCharacters.Add(character);
        }
    }
    private int RemoveFromCombatOrder(Character character) {
        for (int i = 0; i < _combatOrder.Count; i++) {
            if(_combatOrder[i].character == character) {
                int index = i;
                RemoveFromCombatGrid(_combatOrder[i]);
                _leftSideCombatOrder.Remove(_combatOrder[i]);
                _rightSideCombatOrder.Remove(_combatOrder[i]);
                _combatOrder.RemoveAt(i);
                return index;
            }
        }
        return -1;
    }
    private void RemoveFromCombatGrid(CombatCharacter combatCharacter) {
        CombatGrid grid = leftSide;
        if (combatCharacter.side == SIDES.B) {
            grid = rightSide;
        }
        grid.RemoveCharacterFromGrid(combatCharacter.character);
    }
    private List<CombatCharacter> GetTargetCharacters(CombatCharacter sourceCombatCharacter) {
        List<int[]> targetIndexes = GetGridIndexesByCombatTargetType(sourceCombatCharacter.character.characterClass.combatTarget);
        List<CombatCharacter> targets = new List<CombatCharacter>();
        CombatGrid gridToBeChecked = leftSide;
        if (sourceCombatCharacter.side == SIDES.A) {
            gridToBeChecked = rightSide;
        }
        for (int i = 0; i < targetIndexes.Count; i++) {
            bool canBeTargeted = false;
            for (int j = 0; j < targetIndexes[i].Length; j++) {
                Character targetCharacter = gridToBeChecked.slots[targetIndexes[i][j]].character;
                if (targetCharacter != null && !_deadCharacters.Contains(targetCharacter)) { //&& !targets.Contains(targetCharacter)
                    canBeTargeted = true;
                    break;
                } else {
                    if(sourceCombatCharacter.character.characterClass.combatTarget == COMBAT_TARGET.SINGLE_FRONTROW) {
                        if(targetIndexes[i][j] == 0) {
                            targetIndexes[i][j] = 2;
                            j--;
                        }else if (targetIndexes[i][j] == 1) {
                            targetIndexes[i][j] = 3;
                            j--;
                        }
                    }
                }
            }
            if (!canBeTargeted) {
                if (sourceCombatCharacter.character.characterClass.combatTarget == COMBAT_TARGET.FRONTROW && targetIndexes[i][0] == 0) {
                    targetIndexes[i] = new int[] { 2, 3 };
                } else {
                    targetIndexes.RemoveAt(i);
                }
                i--;
            }
        }
        if(targetIndexes.Count > 0) {
            int chosenIndex = UnityEngine.Random.Range(0, targetIndexes.Count);
            for (int j = 0; j < targetIndexes[chosenIndex].Length; j++) {
                Character targetCharacter = gridToBeChecked.slots[targetIndexes[chosenIndex][j]].character;
                if (targetCharacter != null && !_deadCharacters.Contains(targetCharacter)) {
                    //CombatCharacter targetCombatCharacter = targetCharacter.currentCombatCharacter;
                    //if (!targets.Contains(targetCombatCharacter)) {
                    //    targets.Add(targetCombatCharacter);
                    //}
                }
            }
        }
        return targets;
    }
    public List<int> GetTargetIndexesForCurrentAttackByIndex(int index) {
        return rightSide.GetTargetGridIndexesFor(currentAttacker.character, index); //rightSide is just a reference to gain access to combat grid's functions
    }
    private List<int[]> GetGridIndexesByCombatTargetType(COMBAT_TARGET combatTargetType) {
        //CombatGrid referenceGrid = leftSide; //This is only a reference to get the indexes for the targets, must not change the grid itself
        List<int[]> targets = new List<int[]>();
        if(combatTargetType == COMBAT_TARGET.SINGLE) {
            targets.Add(new int[] { 0 });
            targets.Add(new int[] { 1 });
            targets.Add(new int[] { 2 });
            targets.Add(new int[] { 3 });
            //for (int i = 0; i < referenceGrid.slots.Length; i++) {
            //    targets.Add(new int[] { referenceGrid.slots[i].gridNumber });
            //}
        } else if (combatTargetType == COMBAT_TARGET.ALL) {
            targets.Add(new int[] { 0, 1, 2, 3 });
            //int[] indexes = new int[referenceGrid.slots.Length];
            //for (int i = 0; i < indexes.Length; i++) {
            //    indexes[i] = referenceGrid.slots[i].gridNumber;
            //}
            //targets.Add(indexes);
        } else if (combatTargetType == COMBAT_TARGET.COLUMN) {
            targets.Add(new int[] { 0, 2 });
            targets.Add(new int[] { 1, 3 });
            //for (int i = 0; i < referenceGrid.columnReference.GetLength(0); i++) {
            //    int insideLength = referenceGrid.columnReference.GetLength(1);
            //    int[] indexes = new int[insideLength];
            //    for (int j = 0; j < insideLength; j++) {
            //        indexes[j] = referenceGrid.columnReference[i, j];
            //    }
            //    targets.Add(indexes);
            //}
        } else if (combatTargetType == COMBAT_TARGET.ROW) {
            targets.Add(new int[] { 0, 1 });
            targets.Add(new int[] { 2, 3 });
            //for (int i = 0; i < referenceGrid.rowReference.GetLength(0); i++) {
            //    int insideLength = referenceGrid.rowReference.GetLength(1);
            //    int[] indexes = new int[insideLength];
            //    for (int j = 0; j < insideLength; j++) {
            //        indexes[j] = referenceGrid.rowReference[i, j];
            //    }
            //    targets.Add(indexes);
            //}
        } else if (combatTargetType == COMBAT_TARGET.FRONTROW) {
            targets.Add(new int[] { 0, 1 });
            //int length = referenceGrid.rowReference.GetLength(1);
            //int[] indexes = new int[length];
            //for (int i = 0; i < length; i++) {
            //    indexes[i] = referenceGrid.rowReference[0, i]; //front row = 0
            //}
            //targets.Add(indexes);
        } 
        //else if (combatTargetType == COMBAT_TARGET.BACKROW) {
        //    targets.Add(new int[] { 2, 3 });
        //    //int length = referenceGrid.rowReference.GetLength(1);
        //    //int[] indexes = new int[length];
        //    //for (int i = 0; i < length; i++) {
        //    //    indexes[i] = referenceGrid.rowReference[1, i]; //back row = 1
        //    //}
        //    //targets.Add(indexes);
        //} 
        else if (combatTargetType == COMBAT_TARGET.SINGLE_FRONTROW) {
            targets.Add(new int[] { 0 });
            targets.Add(new int[] { 1 });
            //int length = referenceGrid.rowReference.GetLength(1);
            //for (int i = 0; i < length; i++) {
            //    targets.Add(new int[] { referenceGrid.rowReference[0, i] });
            //}
        } 
        //else if (combatTargetType == COMBAT_TARGET.SINGLE_BACKROW) {
        //    targets.Add(new int[] { 2 });
        //    targets.Add(new int[] { 3 });
        //    //int length = referenceGrid.rowReference.GetLength(1);
        //    //for (int i = 0; i < length; i++) {
        //    //    targets.Add(new int[] { referenceGrid.rowReference[1, i] });
        //    //}
        //}
        return targets;
    }
    private void SelectTarget(CombatCharacter combatCharacter) {
        SetPausedState(true);
        currentAttacker = combatCharacter;
        isSelectingTarget = true;
        UIManager.Instance.combatUI.selectTargetIndicatorGO.SetActive(true);
    }
    public void OnSelectTargets(List<CombatCharacter> targets) {
        if(targets != null && targets.Count > 0) {
            selectedTargetCharacters = targets;
        } else {
            selectedTargetCharacters = null;
        }
        isSelectingTarget = false;
        UIManager.Instance.combatUI.selectTargetIndicatorGO.SetActive(false);
        SetPausedState(false);
    }

    #region Trait Effects
    private void ApplyStartCombatTraits() {
        for (int i = 0; i < _combatOrder.Count; i++) {
            CombatCharacter combatCharacter = _combatOrder[i];
            for (int j = 0; j < combatCharacter.character.traitContainer.allTraits.Count; j++) {
                //if(combatCharacter.character.traitContainer.allTraits[j].trigger == TRAIT_TRIGGER.START_OF_COMBAT) {
                //    if (combatCharacter.character.traitContainer.allTraits[j].effects != null) {
                //        for (int k = 0; k < combatCharacter.character.traitContainer.allTraits[j].effects.Count; k++) {
                //            TraitEffect traitEffect = combatCharacter.character.traitContainer.allTraits[j].effects[k];
                //            if (combatCharacter.side == SIDES.A) {
                //                ApplyTraitEffectsOfStartCombat(combatCharacter, _leftSideCombatOrder, _rightSideCombatOrder, traitEffect);
                //            } else {
                //                ApplyTraitEffectsOfStartCombat(combatCharacter, _rightSideCombatOrder, _leftSideCombatOrder, traitEffect);
                //            }
                //        }
                //    }
                //}
            }
        }
        for (int i = 0; i < _combatOrder.Count; i++) {
            _combatOrder[i].UpdateStats();
        }
    }
    private void ApplyTraitEffectsOfStartCombat(CombatCharacter sourceCombatCharacter, List<CombatCharacter> allies, List<CombatCharacter> enemies, TraitEffect traitEffect) {
        if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
            if (traitEffect.hasRequirement) {
                List<CombatCharacter> checkerCharacters = GetCharactersToBeChecked(traitEffect.checker, sourceCombatCharacter, allies, enemies);
                for (int i = 0; i < checkerCharacters.Count; i++) {
                    if (CharacterFitsTraitCriteria(checkerCharacters[i], sourceCombatCharacter, traitEffect)) {
                        ModifyStat(sourceCombatCharacter, traitEffect);
                    }
                }
            } else {
                ModifyStat(sourceCombatCharacter, traitEffect);
            }
        } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ENEMY) {
            //Do nothing, must never have target at the start of combat
        } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ALL_ENEMIES) {
            if (traitEffect.hasRequirement) {
                List<CombatCharacter> checkerCharacters = GetCharactersToBeChecked(traitEffect.checker, sourceCombatCharacter, allies, enemies);
                for (int i = 0; i < checkerCharacters.Count; i++) {
                    if(CharacterFitsTraitCriteria(checkerCharacters[i], sourceCombatCharacter, traitEffect)) {
                        for (int j = 0; j < enemies.Count; j++) {
                            ModifyStat(enemies[j], traitEffect);
                        }
                    }
                }
            } else {
                for (int i = 0; i < enemies.Count; i++) {
                    ModifyStat(enemies[i], traitEffect);
                }
            }
        } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.ALL_IN_COMBAT) {
            if (traitEffect.hasRequirement) {
                List<CombatCharacter> checkerCharacters = GetCharactersToBeChecked(traitEffect.checker, sourceCombatCharacter, allies, enemies);
                for (int i = 0; i < checkerCharacters.Count; i++) {
                    if (CharacterFitsTraitCriteria(checkerCharacters[i], sourceCombatCharacter, traitEffect)) {
                        for (int j = 0; j < enemies.Count; j++) {
                            ModifyStat(enemies[j], traitEffect);
                        }
                        for (int j = 0; j < allies.Count; j++) {
                            ModifyStat(allies[j], traitEffect);
                        }
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
                List<CombatCharacter> checkerCharacters = GetCharactersToBeChecked(traitEffect.checker, sourceCombatCharacter, allies, enemies);
                for (int i = 0; i < checkerCharacters.Count; i++) {
                    if (CharacterFitsTraitCriteria(checkerCharacters[i], sourceCombatCharacter, traitEffect)) {
                        for (int j = 0; j < allies.Count; j++) {
                            ModifyStat(allies[j], traitEffect);
                        }
                    }
                }
            } else {
                for (int i = 0; i < allies.Count; i++) {
                    ModifyStat(allies[i], traitEffect);
                }
            }
        } else if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.OTHER_PARTY_MEMBERS) {
            if (traitEffect.hasRequirement) {
                List<CombatCharacter> checkerCharacters = GetCharactersToBeChecked(traitEffect.checker, sourceCombatCharacter, allies, enemies);
                for (int i = 0; i < checkerCharacters.Count; i++) {
                    if (CharacterFitsTraitCriteria(checkerCharacters[i], sourceCombatCharacter, traitEffect)) {
                        for (int j = 0; j < allies.Count; j++) {
                            if (allies[j] != sourceCombatCharacter) {
                                ModifyStat(allies[j], traitEffect);
                            }
                        }
                    }
                }
            } else {
                for (int i = 0; i < allies.Count; i++) {
                    if (allies[i] != sourceCombatCharacter) {
                        ModifyStat(allies[i], traitEffect);
                    }
                }
            }
        }
    }
    private void ModifyStat(CombatCharacter sourceCombatCharacter, TraitEffect traitEffect) {
        if (traitEffect.isPercentage) {
            if (traitEffect.stat == STAT.ATTACK) {
                sourceCombatCharacter.AdjustMultiplierAttack((int) traitEffect.amount);
            } else if (traitEffect.stat == STAT.SPEED) {
                sourceCombatCharacter.AdjustMultiplierSpeed((int) traitEffect.amount);
            } else if (traitEffect.stat == STAT.HP) {
                sourceCombatCharacter.AdjustMultiplierHP((int) traitEffect.amount);
            } else if (traitEffect.stat == STAT.ALL) {
                sourceCombatCharacter.AdjustMultiplierAttack((int) traitEffect.amount);
                sourceCombatCharacter.AdjustMultiplierSpeed((int) traitEffect.amount);
                sourceCombatCharacter.AdjustMultiplierHP((int) traitEffect.amount);
            }
        } else {
            if (traitEffect.stat == STAT.ATTACK) {
                sourceCombatCharacter.AdjustFlatAttack((int) traitEffect.amount);
            } else if (traitEffect.stat == STAT.SPEED) {
                sourceCombatCharacter.AdjustFlatSpeed((int) traitEffect.amount);
            } else if (traitEffect.stat == STAT.HP) {
                sourceCombatCharacter.AdjustFlatHP((int) traitEffect.amount);
            } else if (traitEffect.stat == STAT.ALL) {
                sourceCombatCharacter.AdjustFlatAttack((int) traitEffect.amount);
                sourceCombatCharacter.AdjustFlatSpeed((int) traitEffect.amount);
                sourceCombatCharacter.AdjustFlatHP((int) traitEffect.amount);
            }
        }
    }
    private List<CombatCharacter> GetCharactersToBeChecked(TRAIT_REQUIREMENT_CHECKER checker, CombatCharacter sourceCombatCharacter,
        List<CombatCharacter> allies, List<CombatCharacter> enemies, List<CombatCharacter> targets = null) {

        List<CombatCharacter> checkerCharacters = new List<CombatCharacter>();
        if(checker == TRAIT_REQUIREMENT_CHECKER.SELF) {
            checkerCharacters.Add(sourceCombatCharacter);
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.ENEMY) {
            checkerCharacters.AddRange(targets);
        } else if (checker == TRAIT_REQUIREMENT_CHECKER.OTHER_PARTY_MEMBERS) {
            checkerCharacters = allies.Where(x => x != sourceCombatCharacter).ToList();
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
    private bool CharacterFitsTraitCriteria(CombatCharacter checkedCharacter, CombatCharacter traitOwner, TraitEffect traitEffect, List<CombatCharacter> targets = null) {
        if (traitEffect.requirementType == TRAIT_REQUIREMENT.TRAIT) {
            if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.AND) {
                //if there is one mismatch, return false already because the separator is AND, otherwise, return true
                if (traitEffect.isNot) {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (checkedCharacter.character.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (!checkedCharacter.character.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return false;
                        }
                    }
                    return true;
                }
            } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                //if there is one match, return true already because the separator is OR, otherwise, return false   
                if (traitEffect.isNot) {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (!checkedCharacter.character.traitContainer.HasTrait(traitEffect.requirements[i])) {
                            return true;
                        }
                    }
                    return false;
                } else {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (checkedCharacter.character.traitContainer.HasTrait(traitEffect.requirements[i])) {
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
                        if (traitEffect.requirements[i].ToLower() == checkedCharacter.character.race.ToString().ToLower()) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (traitEffect.requirements[i].ToLower() != checkedCharacter.character.race.ToString().ToLower()) {
                            return false;
                        }
                    }
                    return true;
                }
            } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                //if there is one match, return true already because the separator is OR, otherwise, return false   
                if (traitEffect.isNot) {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (traitEffect.requirements[i].ToLower() != checkedCharacter.character.race.ToString().ToLower()) {
                            return true;
                        }
                    }
                    return false;
                } else {
                    for (int i = 0; i < traitEffect.requirements.Count; i++) {
                        if (traitEffect.requirements[i].ToLower() == checkedCharacter.character.race.ToString().ToLower()) {
                            return true;
                        }
                    }
                    return false;
                }
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.ROLE) {
            if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.AND) {
                //if there is one mismatch, return false already because the separator is AND, otherwise, return true
                if (traitEffect.isNot) {
                    // for (int i = 0; i < traitEffect.requirements.Count; i++) {
                    //     if (traitEffect.requirements[i].ToLower() == checkedCharacter.character.role.roleType.ToString().ToLower()) {
                    //         return false;
                    //     }
                    // }
                    return true;
                } else {
                    // for (int i = 0; i < traitEffect.requirements.Count; i++) {
                    //     if (traitEffect.requirements[i].ToLower() != checkedCharacter.character.role.roleType.ToString().ToLower()) {
                    //         return false;
                    //     }
                    // }
                    return true;
                }
            } else if (traitEffect.requirementSeparator == TRAIT_REQUIREMENT_SEPARATOR.OR) {
                //if there is one match, return true already because the separator is OR, otherwise, return false   
                if (traitEffect.isNot) {
                    // for (int i = 0; i < traitEffect.requirements.Count; i++) {
                    //     if (traitEffect.requirements[i].ToLower() != checkedCharacter.character.role.roleType.ToString().ToLower()) {
                    //         return true;
                    //     }
                    // }
                    return false;
                } else {
                    // for (int i = 0; i < traitEffect.requirements.Count; i++) {
                    //     if (traitEffect.requirements[i].ToLower() == checkedCharacter.character.role.roleType.ToString().ToLower()) {
                    //         return true;
                    //     }
                    // }
                    return false;
                }
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.FRONTLINE) {
            if (traitEffect.isNot) {
                if (leftSide.slots[2].isOccupied && leftSide.slots[2].character == checkedCharacter.character) {
                    return true;
                } else if (leftSide.slots[3].isOccupied && leftSide.slots[3].character == checkedCharacter.character) {
                    return true;
                } else if (rightSide.slots[2].isOccupied && rightSide.slots[2].character == checkedCharacter.character) {
                    return true;
                } else if (rightSide.slots[3].isOccupied && rightSide.slots[3].character == checkedCharacter.character) {
                    return true;
                }
                return false;
            } else {
                if (leftSide.slots[0].isOccupied && leftSide.slots[0].character == checkedCharacter.character) {
                    return true;
                }else if (leftSide.slots[1].isOccupied && leftSide.slots[1].character == checkedCharacter.character) {
                    return true;
                }else if (rightSide.slots[0].isOccupied && rightSide.slots[0].character == checkedCharacter.character) {
                    return true;
                }else if (rightSide.slots[1].isOccupied && rightSide.slots[1].character == checkedCharacter.character) {
                    return true;
                }
                return false;
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.ADJACENT_ALLIES) {
            if (traitEffect.isNot) {
                CombatGrid side = leftSide;
                if(checkedCharacter.side == SIDES.B) {
                    side = rightSide;
                }
                int adjacent = CombatManager.Instance.GetAdjacentIndexGrid(checkedCharacter.gridNumber);
                if (!side.slots[adjacent].isOccupied || side.slots[adjacent].character == checkedCharacter.character) {
                    return true;
                }
                return false;
            } else {
                CombatGrid side = leftSide;
                if (checkedCharacter.side == SIDES.B) {
                    side = rightSide;
                }
                int adjacent = CombatManager.Instance.GetAdjacentIndexGrid(checkedCharacter.gridNumber);
                if (side.slots[adjacent].isOccupied && side.slots[adjacent].character != checkedCharacter.character) {
                    return true;
                }
                return false;
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.ONLY_1_TARGET) {
            if (traitEffect.isNot) {
                if(targets.Count > 1) {
                    return true;
                }
                return false;
            } else {
                if (targets.Count == 1) {
                    return true;
                }
                return false;
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.EVERY_MISSING_HP_25PCT) {
            if (traitEffect.isNot) {
                float percent = checkedCharacter.maxHP * 0.25f;
                if (checkedCharacter.missingHP < percent) {
                    return true;
                }
                return false;
            } else {
                float percent = checkedCharacter.maxHP * 0.25f;
                if (checkedCharacter.missingHP >= percent) {
                    return true;
                }
                return false;
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.MELEE) {
            if (traitEffect.isNot) {
                if (checkedCharacter.character.characterClass.rangeType != RANGE_TYPE.MELEE) {
                    return true;
                }
                return false;
            } else {
                if (checkedCharacter.character.characterClass.rangeType == RANGE_TYPE.MELEE) {
                    return true;
                }
                return false;
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.RANGED) {
            if (traitEffect.isNot) {
                if (checkedCharacter.character.characterClass.rangeType != RANGE_TYPE.RANGED) {
                    return true;
                }
                return false;
            } else {
                if (checkedCharacter.character.characterClass.rangeType == RANGE_TYPE.RANGED) {
                    return true;
                }
                return false;
            }
        } else if (traitEffect.requirementType == TRAIT_REQUIREMENT.OPPOSITE_SEX) {
            if (traitEffect.isNot) {
                if (traitOwner.character.gender == checkedCharacter.character.gender) {
                    return true;
                }
                return false;
            } else {
                if (traitOwner.character.gender != checkedCharacter.character.gender) {
                    return true;
                }
                return false;
            }
        }
        return false;
    }
    private int GetFinalAttack(CombatCharacter sourceCombatCharacter, CombatCharacter specificTarget, List<CombatCharacter> targets) {
        //TODO: Currently, only percentage amounts are implemented, if there will flat amounts in the future, add it here
        int finalAttack = sourceCombatCharacter.attack;
        float damageIncreasePercentage = 0f;
        for (int i = 0; i < sourceCombatCharacter.character.traitContainer.allTraits.Count; i++) {
            Trait trait = sourceCombatCharacter.character.traitContainer.allTraits[i];
            //if(trait.trigger == TRAIT_TRIGGER.DURING_COMBAT) {
            //    if (trait.effects != null) {
            //        for (int j = 0; j < trait.effects.Count; j++) {
            //            TraitEffect traitEffect = trait.effects[j];
            //            if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF && traitEffect.stat == STAT.ATTACK && traitEffect.damageIdentifier == DAMAGE_IDENTIFIER.DEALT
            //                && WillTraitApplyDuringCombat(traitEffect, sourceCombatCharacter, sourceCombatCharacter, specificTarget, targets)) {
            //                if (traitEffect.requirementType == TRAIT_REQUIREMENT.EVERY_MISSING_HP_25PCT) {
            //                    int percent = (int) (sourceCombatCharacter.maxHP * 0.25f);
            //                    int multiplier = sourceCombatCharacter.missingHP / percent;
            //                    damageIncreasePercentage += (traitEffect.amount * multiplier);
            //                } else {
            //                    damageIncreasePercentage += traitEffect.amount;
            //                }
            //            }
            //        }
            //    }
            //}
        }
        for (int i = 0; i < specificTarget.character.traitContainer.allTraits.Count; i++) {
            Trait trait = specificTarget.character.traitContainer.allTraits[i];
            //if (trait.trigger == TRAIT_TRIGGER.DURING_COMBAT) {
            //    if (trait.effects != null) {
            //        for (int j = 0; j < trait.effects.Count; j++) {
            //            TraitEffect traitEffect = trait.effects[j];
            //            if (traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF && traitEffect.stat == STAT.ATTACK && traitEffect.damageIdentifier == DAMAGE_IDENTIFIER.RECEIVED
            //                && WillTraitApplyDuringCombat(traitEffect, specificTarget, specificTarget, sourceCombatCharacter, targets)) {
            //                if (traitEffect.requirementType == TRAIT_REQUIREMENT.EVERY_MISSING_HP_25PCT) {
            //                    int percent = (int) (specificTarget.maxHP * 0.25f);
            //                    int multiplier = specificTarget.missingHP / percent;
            //                    damageIncreasePercentage += (traitEffect.amount * multiplier);
            //                } else {
            //                    damageIncreasePercentage += traitEffect.amount;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        finalAttack = (int)(finalAttack * (1f + (damageIncreasePercentage / 100f)));
        return finalAttack;
    }
    private bool WillTraitApplyDuringCombat(TraitEffect traitEffect, CombatCharacter traitOwner, CombatCharacter sourceCombatCharacter, CombatCharacter specificTarget, List<CombatCharacter> targets) {
        if (traitEffect.hasRequirement) {
            if (traitEffect.checker == TRAIT_REQUIREMENT_CHECKER.SELF && CharacterFitsTraitCriteria(sourceCombatCharacter, traitOwner, traitEffect, targets)) {
                return true;
            } else if (traitEffect.checker == TRAIT_REQUIREMENT_CHECKER.ENEMY && CharacterFitsTraitCriteria(specificTarget, traitOwner, traitEffect, targets)) {
                return true;
            }
            return false;
        }
        return true;
    }
    #endregion

    #region Items
    private void UseItem(CombatCharacter combatCharacter) {
        //if(combatCharacter.character.isHoldingItem) {
        //    if(combatCharacter.character.tokenInInventory.specialTokenType == SPECIAL_TOKEN.HEALING_POTION) {
        //        if (combatCharacter.character.currentHP < (combatCharacter.maxHP / 2)) {
        //            combatCharacter.RestoreToFullHP();
        //            UIManager.Instance.combatUI.AddCombatLogs(combatCharacter.character.name + " used " + combatCharacter.character.tokenInInventory.tokenName, combatCharacter.side);
        //            combatCharacter.character.ConsumeToken();
        //        }
        //    }
        //}
    }
    #endregion
}

public class CombatCharacter {
    public Character character { get; private set; }
    public SIDES side { get; private set; }
    public int gridNumber { get; private set; }
    public int attack { get; private set; }
    public int maxHP { get; private set; }
    public int speed { get; private set; }

    private int flatAttack;
    private int flatHP;
    private int flatSpeed;
    private int multiplierAttack;
    private int multiplierHP;
    private int multiplierSpeed;

    #region getters/setters
    public int missingHP {
        get { return maxHP - character.currentHP; }
    }
    #endregion

    public CombatCharacter(Character character, int gridNumber, SIDES side) {
        this.character = character;
        this.side = side;
        this.gridNumber = gridNumber;
    }
    public void AdjustFlatAttack(int amount) {
        flatAttack += amount;
    }
    public void AdjustFlatHP(int amount) {
        flatHP += amount;
    }
    public void AdjustFlatSpeed(int amount) {
        flatSpeed += amount;
    }
    public void AdjustMultiplierAttack(int amount) {
        multiplierAttack += amount;
    }
    public void AdjustMultiplierHP(int amount) {
        multiplierHP += amount;
    }
    public void AdjustMultiplierSpeed(int amount) {
        multiplierSpeed += amount;
    }
    public void UpdateStats() {
        UpdateAttack();
        UpdateHP();
        UpdateSpeed();
    }
    public void UpdateSpeed() {
        int totalFlatSpeed = character.speed + flatSpeed;
        speed = totalFlatSpeed + (int)(totalFlatSpeed * (multiplierSpeed / 100f));
    }
    public void UpdateAttack() {
        int totalFlatAttack = character.attackPower + flatAttack;
        attack = totalFlatAttack + (int) (totalFlatAttack * (multiplierAttack / 100f));
    }
    public void UpdateHP() {
        int totalFlatHP = character.maxHP + flatHP;
        maxHP = totalFlatHP + (int) (totalFlatHP * (multiplierHP / 100f));
        character.SetHP(maxHP);
    }
    public void RestoreToFullHP() {
        character.SetHP(maxHP);
    }
}
