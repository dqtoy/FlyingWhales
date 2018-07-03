using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumberyard : StructureObj {

    public Lumberyard() : base() {
        _specificObjectType = LANDMARK_TYPE.LUMBERYARD;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Lumberyard clone = new Lumberyard();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
