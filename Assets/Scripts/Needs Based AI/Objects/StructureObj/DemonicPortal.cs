using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicPortal : StructureObj {

    public DemonicPortal() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.DEMONIC_PORTAL;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

}
