using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonCemetery : StructureObj {
    public SkeletonCemetery() : base() {
        _specificObjectType = LANDMARK_TYPE.SKELETON_CEMETERY;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        SkeletonCemetery clone = new SkeletonCemetery();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
