using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewCombat : MonoBehaviour {
    public CombatGrid leftSide;
    public CombatGrid rightSide;

    private List<Character> _deadCharacters;
    private List<CombatCharacter> _combatOrder;

    private bool _isPaused;

    #region getters/setters
    private bool isCombatEnd {
        get { return leftSide.IsGridEmpty() || rightSide.IsGridEmpty(); }
    }
    #endregion

    public void Initialize() {
        _deadCharacters = new List<Character>();
        _combatOrder = new List<CombatCharacter>();
        leftSide.Initialize();
        rightSide.Initialize();
    }

    private void ResetCombat() {
        leftSide.ResetGrid();
        rightSide.ResetGrid();
        _deadCharacters.Clear();
        _combatOrder.Clear();
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
            chosenGrid.slots[i].character = grid.slots[i].character;
        }
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
            if (leftSide.slots[i].isOccupied) {
                _combatOrder.Add(
                    new CombatCharacter() {
                        character = leftSide.slots[i].character,
                        side = SIDES.A,
                        speed = 0,
                    }
                );
            }
            if (rightSide.slots[i].isOccupied) {
                _combatOrder.Add(
                    new CombatCharacter() {
                        character = rightSide.slots[i].character,
                        side = SIDES.B,
                        speed = 0,
                    }
                );
            }
        }
    }

    public void Fight() {
        TransferSlotsToOrder();
        while (!isCombatEnd) {
            StartCoroutine(StartRound());
        }
    }

    private IEnumerator StartRound() {
        ReorderCombat();
        for (int i = 0; i < _combatOrder.Count; i++) {
            CombatCharacter sourceCombatCharacter = _combatOrder[i];
            //if (sourceCombatCharacter.character.currentHP <= 0) {
            //    _combatOrder.RemoveAt(i);
            //    i--;
            //    continue;
            //}
            //Get and Hit Targets
            List<Character> targetCharacters = GetTargetCharacters(sourceCombatCharacter);
            for (int j = 0; j < targetCharacters.Count; j++) {
                Character target = targetCharacters[j];
                target.AdjustHP(-sourceCombatCharacter.character.attackPower);
                if (target.currentHP <= 0) {
                    AddToDeadCharacters(target);
                }
            }
            if (isCombatEnd) {
                break;
            }
            yield return new WaitWhile(() => _isPaused == true);
            yield return new WaitForSeconds(CombatManager.Instance.updateIntervals);
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
            RemoveFromCombatOrder(character);
        }
    }
    private bool RemoveFromCombatOrder(Character character) {
        for (int i = 0; i < _combatOrder.Count; i++) {
            if(_combatOrder[i].character == character) {
                RemoveFromCombatGrid(_combatOrder[i]);
                _combatOrder.RemoveAt(i);
                return true;
            }
        }
        return false;
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
            for (int j = 0; j < targetIndexes[i].Length; j++) {
                Character targetCharacter = gridToBeChecked.slots[targetIndexes[i][j]].character;
                if (targetCharacter != null && !_deadCharacters.Contains(targetCharacter) && !targets.Contains(targetCharacter)) {
                    targets.Add(targetCharacter);
                }
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
