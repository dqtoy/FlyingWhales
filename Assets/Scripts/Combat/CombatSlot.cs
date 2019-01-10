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
}
