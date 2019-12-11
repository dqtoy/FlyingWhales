using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MentorFeature : RegionFeature {

    public MentorFeature() {
        name = "Mentor";
        description = "This place has a living mentor. It may be able to impart some knowledge to visiting characters.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Overrides
    public override void Activate(Region region) {
        base.Activate(region);
        
    }
    #endregion
}
