using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temple : StructureObj {

    public Temple() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.TEMPLE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }
}
