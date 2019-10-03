using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceFeature : RegionFeature {

	public ExperienceFeature() {
        name = "Experience";
        description = "Invading this region is going to be tough. The process will improve your Minions, increasing all their Levels by 1.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Overrides
    public override void Activate(Region region) {
        base.Activate(region);
        PlayerManager.Instance.player.LevelUpAllMinions();
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "All your minions have levelled up!", () => PlayerUI.Instance.ShowGeneralConfirmation("Congratulations!", "All your minions gained 1 level."));
    }
    #endregion
}
