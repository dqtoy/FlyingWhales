using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rampart : StructureObj {

    public Rampart() : base() {
        _specificObjectType = LANDMARK_TYPE.RAMPART;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Rampart clone = new Rampart();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
