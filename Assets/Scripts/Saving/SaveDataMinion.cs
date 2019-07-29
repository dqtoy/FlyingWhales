using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataMinion {
    public int characterID;
    public int exp;
    public int indexDefaultSort;
    public int unlockedInterventionSlots;
    public PlayerJobAction[] interventionAbilities;
    public SaveDataCombatAbility combatAbility;

    public List<string> traitsToAdd;
}
