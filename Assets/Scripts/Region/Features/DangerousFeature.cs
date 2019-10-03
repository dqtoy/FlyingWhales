using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerousFeature : RegionFeature {

    public DangerousFeature() {
        name = "Dangerous";
        description = "This place is rumored to be extremely dangerous. It poses a moderate risk to invading minions.";
        type = REGION_FEATURE_TYPE.ACTIVE;
    }

    #region Override
    public override void Activate(Region region) {
        base.Activate(region);
        if (Random.Range(0, 100) < 20 && region.assignedMinion != null && !region.assignedMinion.character.isDead) {
            Debug.Log(GameManager.Instance.TodayLogString() + region.assignedMinion.character.name + " died from Dangerous feature at " + region.name);
            region.assignedMinion.Death(name);
        }
    }
    #endregion
}