﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePyramid : StructureObj {
    public ZombiePyramid() : base() {
        _specificObjectType = LANDMARK_TYPE.ZOMBIE_PYRAMID;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        ZombiePyramid clone = new ZombiePyramid();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}