using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GoHomeAction : CharacterAction {
    public GoHomeAction() : base(ACTION_TYPE.GO_HOME) {

    }
    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override CharacterAction Clone() {
        GoHomeAction goHomeAction = new GoHomeAction();
        SetCommonData(goHomeAction);
        goHomeAction.Initialize();
        return goHomeAction;
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        if (party.homeLandmark == null || targetObject.objectLocation.tileLocation.areaOfTile.id != party.homeLandmark.tileLocation.areaOfTile.id) {
            return false;
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    #endregion
}
