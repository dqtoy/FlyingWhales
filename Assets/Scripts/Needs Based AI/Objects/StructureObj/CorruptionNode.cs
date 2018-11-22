using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionNode : StructureObj {

    public CorruptionNode() : base() {
        _specificObjectType = LANDMARK_TYPE.CORRUPTION_NODE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        CorruptionNode clone = new CorruptionNode();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        base.OnAddToLandmark(newLocation);
        newLocation.tileLocation.ScheduleCorruption();
    }
    #endregion
}
