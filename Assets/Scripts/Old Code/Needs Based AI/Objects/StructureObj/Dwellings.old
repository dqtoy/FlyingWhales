using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dwellings : StructureObj {

    public Dwellings() : base() {
        _specificObjectType = LANDMARK_TYPE.DWELLINGS;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Dwellings clone = new Dwellings();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
