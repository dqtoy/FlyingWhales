using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class GoHomeAction : CharacterAction {
    public GoHomeAction() : base(ACTION_TYPE.GO_HOME) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        GoHomeAction goHomeAction = new GoHomeAction();
        SetCommonData(goHomeAction);
        goHomeAction.Initialize();
        return goHomeAction;
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if (party.homeLandmark == null || targetObject.objectLocation.tileLocation.areaOfTile.id != party.homeLandmark.tileLocation.areaOfTile.id) {
            return false;
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    #endregion
}
