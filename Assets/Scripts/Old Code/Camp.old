using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : StructureObj {

    public Camp() : base() {
        _specificObjectType = LANDMARK_TYPE.CAMP;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Camp clone = new Camp();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
