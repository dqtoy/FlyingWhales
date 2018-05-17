using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OakFortification : StructureObj {

    public OakFortification() : base(){
        _specificObjectType = SPECIFIC_OBJECT_TYPE.OAK_FORTIFICATION;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

}
