using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class NewCombat : MonoBehaviour {
    public CombatGrid leftSide;
    public CombatGrid rightSide;

    private List<Character> _deadCharacters;
    private List<CombatCharacter> _combatOrder;
    private List<Action> _endCombatActions;

    public SIDES winningSide { get; private set; }

    private bool _isPaused;

    #region getters/setters
    private bool isCombatEnd {
        get { return leftSide.IsGridEmpty() || rightSide.IsGridEmpty(); }
    }
    #endregion

    public void Initialize() {
        _deadCharacters = new List<Character>();
        _combatOrder = new List<CombatCharacter>();
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
            _combatOrder[i].ReEvaluateSpeed();
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
                _combatOrder.Add(
                    new CombatCharacter() {
                        character = leftSide.slots[i].character,
                        side = SIDES.A,
                        speed = 0,
                    }
                );
                leftSide.slots[i].character.AdjustDoNotDisturb(1);
            }
            if (rightSide.slots[i].isOccupied && !IsInCombatOrder(rightSide.slots[i].character)) {
                _combatOrder.Add(
                    new CombatCharacter() {
                        character = rightSide.slots[i].character,
                        side = SIDES.B,
                        speed = 0,
                    }
                );
                rightSide.slots[i].character.AdjustDoNotDisturb(1);
            }
        }
    }
    private bool IsInCombatOrder(Character character) {
        for (int i = 0; i < _combatOrder.Count; i++) {
            if(_combatOrder[i].character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    public void Fight() {
        StartCoroutine(StartCombatCoroutine());
    }

    private IEnumerator StartCombatCoroutine() {
        TransferSlotsToOrder();
        while (!isCombatEnd) {
            ReorderCombat();
            CombatCharacter previousCombatCharacter = new CombatCharacter();
            for (int i = 0; i < _combatOrder.Count; i++) {
                if (previousCombatCharacter.character != null) {
                    UIManager.Instance.combatUI.UnhighlightAttacker(previousCombatCharacter.character, previousCombatCharacter.side);
                }
                CombatCharacter sourceCombatCharacter = _combatOrder[i];
                previousCombatCharacter = sourceCombatCharacter;
                UIManager.Instance.combatUI.HighlightAttacker(sourceCombatCharacter.character, sourceCombatCharacter.side);
                //Messenger.Broadcast(Signals.HIGHLIGHT_ATTACKER, sourceCombatCharacter.character, sourceCombatCharacter.side);
                UIManager.Instance.combatUI.AddCombatLogs(sourceCombatCharacter.character.name + " will now attack!");
                //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, sourceCombatCharacter.character.name + " will now attack!");

                //if (sourceCombatCharacter.character.currentHP <= 0) {
                //    _combatOrder.RemoveAt(i);
                //    i--;
                //    continue;
                //}
                //Get and Hit Targets
                List<Character> targetCharacters = GetTargetCharacters(sourceCombatCharacter);
                string attackLog = string.Empty;
                if (targetCharacters.Count > 0) {
                    attackLog = sourceCombatCharacter.character.name + " attacks ";
                    int sourceAttack = sourceCombatCharacter.character.attackPower;
                    for (int j = 0; j < targetCharacters.Count; j++) {
                        Character target = targetCharacters[j];
                        if (j == 0) {
                            attackLog += target.name;
                        } else {
                            attackLog += ", " + target.name;
                        }
                        target.AdjustHP(-sourceAttack);
                        if (target.currentHP <= 0) {
                            int removedIndex = RemoveFromCombatOrder(target);
                            if(removedIndex != -1) {
                                AddToDeadCharacters(target);
                                if (removedIndex < i) {
                                    i--;
                                }
                            }

                        }
                    }
                    attackLog += " for " + sourceAttack.ToString() + ".";
                    UIManager.Instance.combatUI.AddCombatLogs(attackLog);
                    //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, attackLog);

                    string deathLog = string.Empty;
                    for (int j = 0; j < targetCharacters.Count; j++) {
                        if (targetCharacters[j].currentHP <= 0) {
                            if (deathLog == string.Empty) {
                                deathLog += targetCharacters[j].name;
                            } else {
                                deathLog += ", " + targetCharacters[j].name;
                            }
                        }
                    }
                    if (deathLog != string.Empty) {
                        deathLog += " is dead!";
                        UIManager.Instance.combatUI.AddCombatLogs(deathLog);
                    }
                    //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, deathLog);
                } else {
                    attackLog = "No more targets! Combat will end!";
                    UIManager.Instance.combatUI.AddCombatLogs(attackLog);
                    //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, attackLog);
                }

                //Messenger.Broadcast(Signals.UNHIGHLIGHT_ATTACKER, sourceCombatCharacter.character, sourceCombatCharacter.side);
                UIManager.Instance.combatUI.UpdateCombatSlotItems();
                //Messenger.Broadcast(Signals.UPDATE_COMBAT_GRIDS);
                yield return new WaitWhile(() => _isPaused == true);
                yield return new WaitForSeconds(2f);
                if (isCombatEnd) {
                    break;
                }
            }
        }
        if (rightSide.IsGridEmpty()) {
            winningSide = SIDES.A;
            UIManager.Instance.combatUI.AddCombatLogs("Left Side Wins!");
            //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, "Left Side Wins!");
        } else {
            winningSide = SIDES.B;
            UIManager.Instance.combatUI.AddCombatLogs("Right Side Wins!");
            //Messenger.Broadcast(Signals.ADD_TO_COMBAT_LOGS, "Right Side Wins!");
        }
        for (int i = 0; i < _combatOrder.Count; i++) {
            _combatOrder[i].character.AdjustDoNotDisturb(-1);
        }
        for (int i = 0; i < _deadCharacters.Count; i++) {
            _deadCharacters[i].AdjustDoNotDisturb(-1);
        }
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

    private void HitTargetCharacters(CombatCharacter sourceCombatCharacter) {
        
    }
    private List<Character> GetTargetCharacters(CombatCharacter sourceCombatCharacter) {
        List<int[]> targetIndexes = GetGridIndexesByCombatTargetType(sourceCombatCharacter.character.characterClass.combatTarget);
        List<Character> targets = new List<Character>();
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
                }
            }
            if (!canBeTargeted) {
                targetIndexes.RemoveAt(i);
                i--;
            }
        }
        int chosenIndex = UnityEngine.Random.Range(0, targetIndexes.Count);
        for (int j = 0; j < targetIndexes[chosenIndex].Length; j++) {
            Character targetCharacter = gridToBeChecked.slots[targetIndexes[chosenIndex][j]].character;
            if (targetCharacter != null && !_deadCharacters.Contains(targetCharacter) && !targets.Contains(targetCharacter)) {
                targets.Add(targetCharacter);
            }
        }
        return targets;
    }
    private List<int[]> GetGridIndexesByCombatTargetType(COMBAT_TARGET combatTargetType) {
        CombatGrid referenceGrid = leftSide; //This is only a reference to get the indexes for the targets, must not change the grid itself
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
        } else if (combatTargetType == COMBAT_TARGET.BACKROW) {
            targets.Add(new int[] { 2, 3 });
            //int length = referenceGrid.rowReference.GetLength(1);
            //int[] indexes = new int[length];
            //for (int i = 0; i < length; i++) {
            //    indexes[i] = referenceGrid.rowReference[1, i]; //back row = 1
            //}
            //targets.Add(indexes);
        } else if (combatTargetType == COMBAT_TARGET.SINGLE_FRONTROW) {
            targets.Add(new int[] { 0 });
            targets.Add(new int[] { 1 });
            //int length = referenceGrid.rowReference.GetLength(1);
            //for (int i = 0; i < length; i++) {
            //    targets.Add(new int[] { referenceGrid.rowReference[0, i] });
            //}
        } else if (combatTargetType == COMBAT_TARGET.SINGLE_BACKROW) {
            targets.Add(new int[] { 2 });
            targets.Add(new int[] { 3 });
            //int length = referenceGrid.rowReference.GetLength(1);
            //for (int i = 0; i < length; i++) {
            //    targets.Add(new int[] { referenceGrid.rowReference[1, i] });
            //}
        }
        return targets;
    }
}

public struct CombatCharacter {
    public Character character;
    public SIDES side;
    public int speed;

    public void ReEvaluateSpeed() {
        speed = character.speed;
    }
}
