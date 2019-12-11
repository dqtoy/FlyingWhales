using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StonyFeature : RegionFeature {

    public StonyFeature() {
        name = "Stony";
        description = "Mines can be built here.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }  
}