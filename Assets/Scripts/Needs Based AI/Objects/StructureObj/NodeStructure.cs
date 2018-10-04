using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeStructure : StructureObj {

    public NodeStructure() : base() {
        _specificObjectType = LANDMARK_TYPE.NODE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        NodeStructure clone = new NodeStructure();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
