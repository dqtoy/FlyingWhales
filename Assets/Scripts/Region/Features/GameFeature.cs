using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFeature : RegionFeature {

    public GameFeature() {
        name = "Game";
        description = "Hunters can obtain food here.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }
}
