using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMine : StructureObj {

    public GoldMine() : base() {
        _specificObjectType = LANDMARK_TYPE.GOLD_MINE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        GoldMine clone = new GoldMine();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
