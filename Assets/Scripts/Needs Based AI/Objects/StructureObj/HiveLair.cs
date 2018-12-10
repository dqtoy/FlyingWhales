using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveLair : StructureObj {
    public HiveLair() : base() {
        _specificObjectType = LANDMARK_TYPE.HIVE_LAIR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        HiveLair clone = new HiveLair();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
