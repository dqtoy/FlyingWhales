using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardedFeature : RegionFeature {

    public WardedFeature() {
        name = "Warded";
        description = "There is a magical ward protecting the area. It poses a small risk to invading minions.";
        type = REGION_FEATURE_TYPE.PASSIVE;
    }
}
