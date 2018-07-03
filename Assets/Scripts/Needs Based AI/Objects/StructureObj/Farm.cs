using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : StructureObj {

    public Farm() : base() {
        _specificObjectType = LANDMARK_TYPE.FARM;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Farm clone = new Farm();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
