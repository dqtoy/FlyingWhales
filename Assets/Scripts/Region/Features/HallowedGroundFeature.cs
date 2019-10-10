﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallowedGroundFeature : RegionFeature {

    public HallowedGroundFeature() {
        name = "Hallowed Ground";
        description = "This place has a divine protection. Demonic structures cannot be erected here until the Hallowed Ground has been defiled.";
        type = REGION_FEATURE_TYPE.PASSIVE;
    }
}