using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnowledgeFeature : RegionFeature {

	public KnowledgeFeature() {
        name = "Knowledge";
        description = "This place contains some secret knowledge. Your Minion may learn a new Skill after invading this region.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Overrides
    public override void Activate(Region region) {
        base.Activate(region);
        COMBAT_ABILITY[] skills = Utilities.GetEnumValues<COMBAT_ABILITY>();
        CombatAbility newAbility = PlayerManager.Instance.CreateNewCombatAbility(skills[Random.Range(1, skills.Length)]);
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Combat Ability: " + newAbility.name, () => PlayerUI.Instance.newMinionAbilityUI.ShowNewMinionAbilityUI(newAbility));
    }
    #endregion
}
