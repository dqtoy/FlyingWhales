using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OakLumberyard : StructureObj {

    public OakLumberyard() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.OAK_LUMBERYARD;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }
}
