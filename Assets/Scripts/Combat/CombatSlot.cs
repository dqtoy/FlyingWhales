using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSlot {
    public int gridNumber;
    public Character character;

    #region getters/setters
    public bool isOccupied {
        get { return character != null; }
    }
    #endregion
    public void OccupySlot(Character character) {
        this.character = character;
    }
    public void ResetSlot() {
        character = null;
    }
}
