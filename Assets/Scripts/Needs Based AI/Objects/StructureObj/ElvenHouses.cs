using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElvenHouses : StructureObj {

    public ElvenHouses() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.ELVEN_HOUSES;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        ElvenHouses clone = new ElvenHouses();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
