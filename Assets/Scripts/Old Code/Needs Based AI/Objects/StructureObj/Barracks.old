using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : StructureObj {

    public Barracks() : base() {
        _specificObjectType = LANDMARK_TYPE.BARRACKS;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Barracks clone = new Barracks();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
