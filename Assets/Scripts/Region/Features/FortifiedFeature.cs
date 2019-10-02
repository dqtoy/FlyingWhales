using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortifiedFeature : RegionFeature {

    public FortifiedFeature() {
        name = "Fortified";
        description = "This region is heavily defended. It poses a moderate risk to invading minions.";
        type = REGION_FEATURE_TYPE.PASSIVE;
    }
}

