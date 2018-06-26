using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDen : StructureObj {

	public MonsterDen() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.MONSTER_DEN;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        MonsterDen clone = new MonsterDen();
        SetCommonData(clone);
        return clone;
    }
    #endregion
}
