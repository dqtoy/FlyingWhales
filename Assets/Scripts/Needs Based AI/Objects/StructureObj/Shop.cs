using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : StructureObj {

    public Shop() : base() {
        _specificObjectType = LANDMARK_TYPE.SHOP;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Shop clone = new Shop();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
