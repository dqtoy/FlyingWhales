using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSourceFeature : TileFeature {

    public MetalSourceFeature() {
        name = "Metal Source";
        description = "Provides Metal";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }
}
