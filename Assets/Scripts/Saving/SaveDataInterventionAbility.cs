using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataInterventionAbility {
    public INTERVENTION_ABILITY abilityType;
    public int level;

    public void Save(PlayerJobActionSlot slot) {
        if(slot.ability != null) {
            abilityType = slot.ability.abilityType;
        } else {
            abilityType = INTERVENTION_ABILITY.NONE;
        }
        level = slot.level;
    }
    public PlayerJobActionSlot Load() {
        PlayerJobActionSlot slot = new PlayerJobActionSlot();
        slot.SetLevel(level);
        if (abilityType != INTERVENTION_ABILITY.NONE) {
            PlayerJobAction ability = PlayerManager.Instance.CreateNewInterventionAbility(abilityType);
            slot.SetAbility(ability);
        }
        return slot;
    }
}
