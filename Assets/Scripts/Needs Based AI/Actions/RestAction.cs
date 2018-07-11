using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RestAction : CharacterAction {
    public RestAction() : base(ACTION_TYPE.REST) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if (party.IsFull(NEEDS.ENERGY)) {
            EndAction(party, targetObject);
        }
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        //Filter: Residents of this Structure
        if (targetObject is StructureObj) {
            BaseLandmark landmark = (targetObject as StructureObj).objectLocation;
            if (landmark.charactersWithHomeOnLandmark.Contains(party.mainCharacter as ECS.Character)) {
                return true;
            }
        }
        return false;
    }
    public override CharacterAction Clone() {
        RestAction restAction = new RestAction();
        SetCommonData(restAction);
        restAction.Initialize();
        return restAction;
    }
    #endregion
}
