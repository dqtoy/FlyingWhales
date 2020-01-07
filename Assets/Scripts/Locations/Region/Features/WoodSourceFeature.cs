using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodSourceFeature : TileFeature {

    public WoodSourceFeature() {
        name = "Wood Source";
        description = "Provides wood.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }  
}