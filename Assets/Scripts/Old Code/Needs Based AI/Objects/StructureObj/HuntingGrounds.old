using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingGrounds : StructureObj {

	public HuntingGrounds() : base() {
        _specificObjectType = LANDMARK_TYPE.HUNTING_GROUNDS;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        HuntingGrounds clone = new HuntingGrounds();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
