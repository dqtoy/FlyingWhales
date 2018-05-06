using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildPigs : StructureObj {

	public WildPigs() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.WILD_PIGS;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        WildPigs clone = new WildPigs();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
