using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpKennel : StructureObj {
    public ImpKennel() : base() {
        _specificObjectType = LANDMARK_TYPE.IMP_KENNEL;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        ZombiePyramid clone = new ZombiePyramid();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
