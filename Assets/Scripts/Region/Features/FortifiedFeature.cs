using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortifiedFeature : RegionFeature {

    public FortifiedFeature() {
        name = "Fortified";
        description = "This region is heavily defended. It poses a moderate risk to invading minions.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Override
    public override void Activate(Region region) {
        base.Activate(region);
        if (Random.Range(0, 100) < 15 && region.assignedMinion != null && !region.assignedMinion.character.isDead) {
            Debug.Log(GameManager.Instance.TodayLogString() + region.assignedMinion.character.name + " died from Fortified feature at " + region.name);
            region.assignedMinion.Death(name);
        }
    }
    #endregion
}

