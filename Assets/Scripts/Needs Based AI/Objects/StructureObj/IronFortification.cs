using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronFortification : StructureObj {

    public IronFortification() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.IRON_FORTIFICATION;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }
}
