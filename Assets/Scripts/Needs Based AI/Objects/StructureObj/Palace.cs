using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Palace : StructureObj {

    //private BuildStructureQuest activeBuildStructureQuest = null;

    public Palace() : base() {
        _specificObjectType = LANDMARK_TYPE.PALACE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Palace clone = new Palace();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
