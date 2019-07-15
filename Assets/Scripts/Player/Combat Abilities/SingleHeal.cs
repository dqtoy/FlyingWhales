using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleHeal : CombatAbility {

	public SingleHeal() {
        name = "Single Heal";
        abilityTags.Add(ABILITY_TAG.MAGIC);
        abilityTags.Add(ABILITY_TAG.SUPPORT);
    }
}
