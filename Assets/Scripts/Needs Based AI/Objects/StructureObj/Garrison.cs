using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garrison : StructureObj {

    public int cooldown;

    public Garrison() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.GARRISON;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Garrison clone = new Garrison();
        SetCommonData(clone);
        return clone;
    }
    #endregion

    public void AdjustCooldown(int adjustment) {
        cooldown += adjustment;
        if (cooldown == 0) {
            if (_currentState.stateName == "Preparing") {
                ObjectState readyState = GetState("Ready");
                ChangeState(readyState);
                ResetCooldown();
            }
        } else if (cooldown == 30) {
            if (_currentState.stateName == "Ready") {
                ObjectState preparingState = GetState("Preparing");
                ChangeState(preparingState);
            }
        }
    }
    public void ResetCooldown() {
        cooldown = 30;
    }
}
