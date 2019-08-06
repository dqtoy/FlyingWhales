using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataInterventionAbility {
    public INTERVENTION_ABILITY abilityType;
    public int lvl;

    public void Save(PlayerJobAction ability) {
        abilityType = ability.abilityType;
        lvl = ability.level;
    }
    public void Load(Minion minion) {
        PlayerJobAction ability = PlayerManager.Instance.CreateNewInterventionAbility(abilityType);
        minion.AddInterventionAbility(ability);
        ability.SetLevel(lvl);
    }
}
