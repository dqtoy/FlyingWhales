using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObject : IWorldObject {

    public string name { get; private set; }
    public SPECIAL_OBJECT_TYPE specialObjType { get; private set; }

    #region getters/setters
    public string worldObjectName {
        get { return name; }
    }
    #endregion

    public SpecialObject(SPECIAL_OBJECT_TYPE specialObjType) {
        this.specialObjType = specialObjType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(specialObjType.ToString());
    }

    #region Virtuals
    public virtual void Obtain() { }
    #endregion
}

public class SaveDataSpecialObject : SaveDataWorldObject {
    public SPECIAL_OBJECT_TYPE specialObjType;

    public override void Save(IWorldObject worldObject) {
        base.Save(worldObject);
        if(worldObject is SpecialObject) {
            specialObjType = (worldObject as SpecialObject).specialObjType;
        }
    }

    public override IWorldObject Load() {
        return new SpecialObject(specialObjType);
    }
}
