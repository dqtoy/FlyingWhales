using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonCave : StructureObj {
    public DragonCave() : base() {
        _specificObjectType = LANDMARK_TYPE.DRAGON_CAVE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        DragonCave clone = new DragonCave();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
