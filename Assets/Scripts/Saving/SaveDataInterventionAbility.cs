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
    public void Load(Player player) {
        PlayerJobAction ability = PlayerManager.Instance.CreateNewInterventionAbility(abilityType);
        player.GainNewInterventionAbility(ability);
        ability.SetLevel(lvl);
    }
}
