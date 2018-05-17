using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lair : StructureObj {

	public Lair() : base() {
        //_specificObjectType = SPECIFIC_OBJECT_TYPE.LAIR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Lair clone = new Lair();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
