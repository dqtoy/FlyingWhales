using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackLandmarkAction : CharacterAction {

    public AttackLandmarkAction() : base(ACTION_TYPE.ATTACK_LANDMARK) {

    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (targetObject is StructureObj) {
            StructureObj structure = targetObject as StructureObj;
            structure.AdjustHP(-10);
            if (structure.currentHP <= 0) {
                EndAction(party, targetObject);
            }
        }
    }
    public override CharacterAction Clone() {
        AttackLandmarkAction action = new AttackLandmarkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
