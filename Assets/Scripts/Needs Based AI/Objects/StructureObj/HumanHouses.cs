using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanHouses : StructureObj {

    public HumanHouses() : base() {
        _specificObjectType = LANDMARK_TYPE.HUMAN_HOUSES;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        HumanHouses clone = new HumanHouses();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
