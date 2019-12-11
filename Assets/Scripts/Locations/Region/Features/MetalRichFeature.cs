using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalRichFeature : RegionFeature {

    public MetalRichFeature() {
        name = "Metal-Rich";
        description = "Mines can be built here.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }
}
