using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnatcherDemonsLair : StructureObj {

    public SnatcherDemonsLair() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.SNATCHER_DEMONS_LAIR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        SnatcherDemonsLair clone = new SnatcherDemonsLair();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
