using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Houses : StructureObj {

    public Houses() : base() {
        _specificObjectType = LANDMARK_TYPE.HOUSES;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Houses clone = new Houses();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
