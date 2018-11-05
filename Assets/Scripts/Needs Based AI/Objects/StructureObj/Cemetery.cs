using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cemetery : StructureObj {
    public Cemetery() : base() {
        _specificObjectType = LANDMARK_TYPE.CEMETERY;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Cemetery clone = new Cemetery();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
