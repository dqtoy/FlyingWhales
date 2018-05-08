using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pub : StructureObj {

	public Pub() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.PUB;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Pub clone = new Pub();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
