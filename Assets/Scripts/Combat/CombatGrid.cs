using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrid : MonoBehaviour {
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
            if (_slots[i].character != null && _slots[i].character.id == character.id){
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
    public bool AssignCharacterToGrid(Character character, int slotIndex, bool overwrite) {
        if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.ALL) {
            if (overwrite) {
                for (int i = 0; i < _slots.Length; i++) {
                    _slots[i].character = character;
                }
            } else {
                for (int i = 0; i < _slots.Length; i++) {
                    if (_slots[i].isOccupied) {
                        return false;
                    }
                }
                for (int i = 0; i < _slots.Length; i++) {
                    _slots[i].character = character;
                }
            }
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.COLUMN) {
            if (overwrite) {
                for (int i = 0; i < _columnReference.Length; i++) {
                    if (_slots[_columnReference[i, 0]].gridNumber == slotIndex || _slots[_columnReference[i, 1]].gridNumber == slotIndex) {
                        _slots[_columnReference[i, 0]].character = character;
                        _slots[_columnReference[i, 1]].character = character;
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < _columnReference.Length; i++) {
                    if (_slots[_columnReference[i, 0]].gridNumber == slotIndex || _slots[_columnReference[i, 1]].gridNumber == slotIndex) {
                        if (!_slots[_columnReference[i, 0]].isOccupied && !_slots[_columnReference[i, 1]].isOccupied) {
                            _slots[_columnReference[i, 0]].character = character;
                            _slots[_columnReference[i, 1]].character = character;
                            return true;
                        }
                    }
                }
            }
            return false;
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.ROW) {
            if (overwrite) {
                for (int i = 0; i < _rowReference.Length; i++) {
                    if (_slots[_rowReference[i, 0]].gridNumber == slotIndex || _slots[_rowReference[i, 1]].gridNumber == slotIndex) {
                        _slots[_rowReference[i, 0]].character = character;
                        _slots[_rowReference[i, 1]].character = character;
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < _rowReference.Length; i++) {
                    if (_slots[_rowReference[i, 0]].gridNumber == slotIndex || _slots[_rowReference[i, 1]].gridNumber == slotIndex) {
                        if (!_slots[_rowReference[i, 0]].isOccupied && !_slots[_rowReference[i, 1]].isOccupied) {
                            _slots[_rowReference[i, 0]].character = character;
                            _slots[_rowReference[i, 1]].character = character;
                            return true;
                        }
                    }
                }
            }
            return false;
        } else if (character.characterClass.occupiedTileType == COMBAT_OCCUPIED_TILE.SINGLE) {
            if (overwrite) {
                _slots[slotIndex].character = character;
            } else {
                if (!_slots[slotIndex].isOccupied) {
                    _slots[slotIndex].character = character;
                    return true;
                }
                return false;
            }
        }
        return true;
    }
}
