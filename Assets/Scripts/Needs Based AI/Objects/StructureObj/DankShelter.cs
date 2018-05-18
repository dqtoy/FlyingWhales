using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DankShelter : StructureObj {

	public DankShelter() : base() {
        //_specificObjectType = SPECIFIC_OBJECT_TYPE.DANK_SHELTER;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        DankShelter clone = new DankShelter();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
