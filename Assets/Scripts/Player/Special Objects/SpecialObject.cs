using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObject : IWorldObject {

    public string name { get; private set; }
    public SPECIAL_OBJECT_TYPE specialObjType { get; private set; }

    public SpecialObject(SPECIAL_OBJECT_TYPE specialObjType) {
        this.specialObjType = specialObjType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(specialObjType.ToString());
    }

    #region Virtuals
    public virtual void Obtain() { }
    #endregion
}
