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
        INTERVENTION_ABILITY[] spells = Utilities.GetEnumValues<INTERVENTION_ABILITY>();
        PlayerJobAction newAbility = PlayerManager.Instance.CreateNewInterventionAbility(spells[Random.Range(1, spells.Length)]);
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Spell: " + newAbility.name, () => PlayerUI.Instance.newMinionAbilityUI.ShowNewMinionAbilityUI(newAbility));
    }
    #endregion
}
