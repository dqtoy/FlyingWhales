using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummoningCircle : StructureObj {

    public SummoningCircle() : base() {
        _specificObjectType = LANDMARK_TYPE.SUMMONING_CIRCLE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        SummoningCircle clone = new SummoningCircle();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
