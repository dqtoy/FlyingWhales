using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dens : StructureObj {

    public Dens() : base() {
        _specificObjectType = LANDMARK_TYPE.DENS;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Dens clone = new Dens();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
