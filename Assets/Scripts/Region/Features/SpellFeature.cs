using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellFeature : RegionFeature {

    public SpellFeature() {
        name = "Spell";
        description = "This place contains some secret knowledge. You may obtain a new Spell after invading this region.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Overrides
    public override void Activate(Region region) {
        base.Activate(region);
        INTERVENTION_ABILITY[] spells = PlayerManager.Instance.allInterventionAbilities;
        PlayerJobAction newAbility = PlayerManager.Instance.CreateNewInterventionAbility(spells[Random.Range(0, spells.Length)]);
        if (PlayerManager.Instance.player.HasEmptyInterventionSlot()) {
            PlayerManager.Instance.player.GainNewInterventionAbility(newAbility);
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Spell: " + newAbility.name, null);
        } else {
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Spell: " + newAbility.name, () => PlayerManager.Instance.player.GainNewInterventionAbility(newAbility));
        }
        
    }
    #endregion
}
