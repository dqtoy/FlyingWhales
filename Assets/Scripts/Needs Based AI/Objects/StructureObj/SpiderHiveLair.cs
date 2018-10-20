using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderHiveLair : StructureObj {
    public SpiderHiveLair() : base() {
        _specificObjectType = LANDMARK_TYPE.SPIDER_HIVE_LAIR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        SpiderHiveLair clone = new SpiderHiveLair();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
