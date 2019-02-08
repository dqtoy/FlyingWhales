using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrid {
    private CombatSlot[] _slots;
    private int[,] _columnReference;
    private int[,] _rowReference;

    #region getters/setters
    public CombatSlot[] slots {
        get { return _slots; }
    }
    public int[,] rowReference {
        get { return _rowReference; }
    }
    public int[,] columnReference {
        get { return _columnReference; }
    }
    #endregion

    public void Initialize() {
        _columnReference = new int[,] { { 0, 2 }, { 1, 3 } };
        _rowReference = new int[,] { { 0, 1 }, { 2, 3 } };
        _slots = new CombatSlot[4];
        for (int i = 0; i < _slots.Length; i++) {
            _slots[i] = new CombatSlot() { gridNumber = i };
        }
    }
    public void ResetGrid() {
        for (int i = 0; i < _slots.Length; i++) {
            _slots[i].ResetSlot();
        }
    }
    public void RemoveCharacterFromGrid(Character character) {
        for (int i = 0; i < _slots.Length; i++) {
            if (_slots[i].isOccupied && _slots[i].character.id == character.id){
                _slots[i].character = null;
            }
        }
    }
    public bool IsGridEmpty() {
        for (int i = 0; i < _slots.Length; i++) {
            if (_slots[i].character != null) {
                return false;
            }
        }
        return true;
    }
    public bool IsCharacterInGrid(Character character) {
        for (int i = 0; i < _slots.Length; i++) {
            if (_slots[i].isOccupied && _slots[i].character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    public bool IsPositionFull(COMBAT_POSITION position) {
        int rowIndex = 0;
        if (position == COMBAT_POSITION.BACKLINE) {
            rowIndex = 1;
        }
        for (int i = 0; i < _rowReference.GetLength(1); i++) {
            if (!_slots[_rowReference[rowIndex, i]].isOccupied) {
                return false;
            }
        }
        return true;
    }
    public bool AssignCharacterToGrid(Character character, int slotIndex, bool overwrite) {
        if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.ALL) {
            if (overwrite) {
                for (int i = 0; i < _slots.Length; i++) {
                    _slots[i].OccupySlot(character);
                }
            } else {
                for (int i = 0; i < _slots.Length; i++) {
                    if (_slots[i].isOccupied) {
                        return false;
                    }
                }
                for (int i = 0; i < _slots.Length; i++) {
                    _slots[i].OccupySlot(character);
                }
            }
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.COLUMN) {
            if (overwrite) {
                for (int i = 0; i < _columnReference.GetLength(0); i++) {
                    if (_slots[_columnReference[i, 0]].gridNumber == slotIndex || _slots[_columnReference[i, 1]].gridNumber == slotIndex) {
                        if(_slots[_columnReference[i, 0]].isOccupied) {
                            RemoveCharacterFromGrid(_slots[_columnReference[i, 0]].character);
                        }
                        if (_slots[_columnReference[i, 1]].isOccupied) {
                            RemoveCharacterFromGrid(_slots[_columnReference[i, 1]].character);
                        }
                        _slots[_columnReference[i, 0]].OccupySlot(character);
                        _slots[_columnReference[i, 1]].OccupySlot(character);
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < _columnReference.GetLength(0); i++) {
                    if (_slots[_columnReference[i, 0]].gridNumber == slotIndex || _slots[_columnReference[i, 1]].gridNumber == slotIndex) {
                        if (!_slots[_columnReference[i, 0]].isOccupied && !_slots[_columnReference[i, 1]].isOccupied) {
                            _slots[_columnReference[i, 0]].OccupySlot(character);
                            _slots[_columnReference[i, 1]].OccupySlot(character);
                            return true;
                        }
                    }
                }
            }
            return false;
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.ROW) {
            if (overwrite) {
                for (int i = 0; i < _rowReference.GetLength(0); i++) {
                    if (_slots[_rowReference[i, 0]].gridNumber == slotIndex || _slots[_rowReference[i, 1]].gridNumber == slotIndex) {
                        if (_slots[_rowReference[i, 0]].isOccupied) {
                            RemoveCharacterFromGrid(_slots[_rowReference[i, 0]].character);
                        }
                        if (_slots[_rowReference[i, 1]].isOccupied) {
                            RemoveCharacterFromGrid(_slots[_rowReference[i, 1]].character);
                        }
                        _slots[_rowReference[i, 0]].OccupySlot(character);
                        _slots[_rowReference[i, 1]].OccupySlot(character);
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < _rowReference.GetLength(0); i++) {
                    if (_slots[_rowReference[i, 0]].gridNumber == slotIndex || _slots[_rowReference[i, 1]].gridNumber == slotIndex) {
                        if (!_slots[_rowReference[i, 0]].isOccupied && !_slots[_rowReference[i, 1]].isOccupied) {
                            _slots[_rowReference[i, 0]].OccupySlot(character);
                            _slots[_rowReference[i, 1]].OccupySlot(character);
                            return true;
                        }
                    }
                }
            }
            return false;
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.SINGLE) {
            if (overwrite) {
                if (_slots[slotIndex].isOccupied) {
                    RemoveCharacterFromGrid(_slots[slotIndex].character);
                }
                _slots[slotIndex].OccupySlot(character);
            } else {
                if (!_slots[slotIndex].isOccupied) {
                    _slots[slotIndex].OccupySlot(character);
                    return true;
                }
                return false;
            }
        }
        return true;
    }
    public bool AssignCharacterToGrid(Character character) {
        if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.ALL) {
            for (int i = 0; i < _slots.Length; i++) {
                if (_slots[i].isOccupied) {
                    return false;
                }
            }
            for (int i = 0; i < _slots.Length; i++) {
                _slots[i].OccupySlot(character);
            }
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.COLUMN) {
            for (int i = 0; i < _columnReference.GetLength(0); i++) {
                if (!_slots[_columnReference[i, 0]].isOccupied && !_slots[_columnReference[i, 1]].isOccupied) {
                    _slots[_columnReference[i, 0]].OccupySlot(character);
                    _slots[_columnReference[i, 1]].OccupySlot(character);
                    return true;
                }
            }
            return false;
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.ROW) {
            for (int i = 0; i < _rowReference.GetLength(0); i++) {
                if (!_slots[_rowReference[i, 0]].isOccupied && !_slots[_rowReference[i, 1]].isOccupied) {
                    _slots[_rowReference[i, 0]].OccupySlot(character);
                    _slots[_rowReference[i, 1]].OccupySlot(character);
                    return true;
                }
            }
            return false;
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.SINGLE) {
            int rowIndex = 0;
            if(character.characterClass.combatPosition == COMBAT_POSITION.BACKLINE) {
                rowIndex = 1;
            }
            for (int i = 0; i < _rowReference.GetLength(1); i++) {
                if (!_slots[_rowReference[rowIndex, i]].isOccupied) {
                    _slots[_rowReference[rowIndex, i]].OccupySlot(character);
                    return true;
                }
            }
            return false;
        }
        return true;
    }
    public List<int> GetTargetGridIndexesFor(Character character, int index) {
        List<int> targetIndexes = new List<int>();
        COMBAT_TARGET combatTargetType = character.characterClass.combatTarget;
        if (combatTargetType == COMBAT_TARGET.SINGLE) {
            targetIndexes.Add(index);
        } else if (combatTargetType == COMBAT_TARGET.ALL) {
            targetIndexes.Add(0);
            targetIndexes.Add(1);
            targetIndexes.Add(2);
            targetIndexes.Add(3);
        } else if (combatTargetType == COMBAT_TARGET.COLUMN) {
            if (index == 0 || index == 2) {
                targetIndexes.Add(0);
                targetIndexes.Add(2);
            } else {
                targetIndexes.Add(1);
                targetIndexes.Add(3);
            }
        } else if (combatTargetType == COMBAT_TARGET.ROW) {
            if (index == 0 || index == 1) {
                targetIndexes.Add(0);
                targetIndexes.Add(1);
            } else {
                targetIndexes.Add(2);
                targetIndexes.Add(3);
            }
        } else if (combatTargetType == COMBAT_TARGET.FRONTROW) {
            if (index == 0 || index == 1) {
                targetIndexes.Add(0);
                targetIndexes.Add(1);
            }
        } 
        //else if (combatTargetType == COMBAT_TARGET.BACKROW) {
        //    if (index == 2 || index == 3) {
        //        targetIndexes.Add(2);
        //        targetIndexes.Add(3);
        //    }
        //} 
        else if (combatTargetType == COMBAT_TARGET.SINGLE_FRONTROW) {
            if (index == 0 || index == 1) {
                targetIndexes.Add(index);
            }
        } 
        //else if (combatTargetType == COMBAT_TARGET.SINGLE_BACKROW) {
        //    if (index == 2 || index == 3) {
        //        targetIndexes.Add(index);
        //    }
        //}
        return targetIndexes;
    }
}
