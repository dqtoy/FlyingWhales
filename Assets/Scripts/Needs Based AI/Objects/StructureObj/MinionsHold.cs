using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionsHold : StructureObj {

    public MinionsHold() : base() {
        _specificObjectType = LANDMARK_TYPE.MINIONS_HOLD;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        MinionsHold clone = new MinionsHold();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
