using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyAction : CharacterAction {

    public PartyAction() : base(ACTION_TYPE.PARTY) {
        _actionData.actionName = "Partying";
        _actionData.advertisedFun = 50;
        _actionData.providedFun = 50;
        SetDuration(20);
    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {

        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override CharacterAction Clone() {
        PartyAction action = new PartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }

    public override IObject GetTargetObject(CharacterParty sourceParty) {
        for (int i = 0; i < sourceParty.homeLandmark.tileLocation.areaOfTile.landmarks.Count; i++) {
            BaseLandmark currLandmark = sourceParty.homeLandmark.tileLocation.areaOfTile.landmarks[i];
            if (currLandmark.HasEventOfType(GAME_EVENT.PARTY_EVENT)) {
                return currLandmark.landmarkObj;
            }
        }
        return null;
    }
    #endregion
}
