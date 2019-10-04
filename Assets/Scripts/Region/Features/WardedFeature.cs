using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardedFeature : RegionFeature {

    public WardedFeature() {
        name = "Warded";
        description = "There is a magical ward protecting the area. It poses a small risk to invading minions.";
        type = REGION_FEATURE_TYPE.ACTIVE;
    }

    #region Override
    public override void Activate(Region region) {
        base.Activate(region);
        if (Random.Range(0, 100) < 10 && region.assignedMinion != null && !region.assignedMinion.character.isDead) {
            Debug.Log(GameManager.Instance.TodayLogString() + region.assignedMinion.character.name + " died from Warded feature at " + region.name);
            region.assignedMinion.Death(name);
        }
    }
    #endregion
}
