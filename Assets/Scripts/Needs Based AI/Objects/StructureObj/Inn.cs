using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inn : StructureObj {

	public Inn() : base() {
        _specificObjectType = LANDMARK_TYPE.INN;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Inn clone = new Inn();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        base.OnAddToLandmark(newLocation);
        newLocation.CreateQuestBoard();
    }
    #endregion
}
