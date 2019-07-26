using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamestrike : CombatAbility {

    private int _damage;
    public Flamestrike() : base(COMBAT_ABILITY.FLAMESTRIKE) {
        description = "Deal AOE damage in the surrounding area.";
        abilityTags.Add(ABILITY_TAG.MAGIC);
        abilityRadius = 3;
        cooldown = 100;
        _currentCooldown = 100;
        _damage = 1000;
    }

    #region Overrides
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            abilityRadius = 3;
        } else if (lvl == 2) {
            abilityRadius = 4;
        } else if (lvl == 3) {
            abilityRadius = 5;
        }
    }
    public override void ActivateAbility(List<IPointOfInterest> targetPOIs) {
        for (int i = 0; i < targetPOIs.Count; i++) {
            if (targetPOIs[i] is Character) {
                Character character = targetPOIs[i] as Character;
                character.AdjustHP(-_damage, true, source: this);
            }
        }
        base.ActivateAbility(targetPOIs);
    }
    #endregion
}
