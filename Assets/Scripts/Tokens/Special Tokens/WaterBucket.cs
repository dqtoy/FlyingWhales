using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBucket : SpecialToken {

    public WaterBucket() : base(SPECIAL_TOKEN.WATER_BUCKET, 100) {
        uses = 5;
        createsObjectWhenDropped = false;
    }
}
