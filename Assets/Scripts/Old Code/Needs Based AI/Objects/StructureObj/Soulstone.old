using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soulstone : StructureObj {

	public Soulstone() : base() {
        //_specificObjectType = SPECIFIC_OBJECT_TYPE.SOULSTONE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Soulstone clone = new Soulstone();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
