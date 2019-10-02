using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerousFeature : RegionFeature {

    public DangerousFeature() {
        name = "Dangerous";
        description = "This place is rumored to be extremely dangerous. It poses a moderate risk to invading minions.";
        type = REGION_FEATURE_TYPE.PASSIVE;
    }
}