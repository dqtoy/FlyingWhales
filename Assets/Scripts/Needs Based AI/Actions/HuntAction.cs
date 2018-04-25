using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class HuntAction : CharacterAction {
    public HuntAction(ObjectState state): base(state, ACTION_TYPE.HUNT) {

    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < actionData.successRate) {
            ActionSuccess();
            GiveReward(NEEDS.FULLNESS, character);
            GiveReward(NEEDS.ENERGY, character);
            if(character.role.IsFull(NEEDS.FULLNESS)){
                EndAction(character);
            }
        } else {
            ActionFail();
            GiveReward(NEEDS.ENERGY, character);
        }
    }
    public override CharacterAction Clone() {
        HuntAction huntAction = new HuntAction(_state);
        SetCommonData(huntAction);
        return huntAction;
    }
    #endregion
}