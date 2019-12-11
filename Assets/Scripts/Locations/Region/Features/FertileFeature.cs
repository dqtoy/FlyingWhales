using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FertileFeature : RegionFeature {

    public FertileFeature() {
        name = "Fertile";
        description = "Farms can be built here.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }  
}