using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class HuntAction : CharacterAction {
    public HuntAction(): base(ACTION_TYPE.HUNT) {

    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if(party is CharacterParty) {
            CharacterParty characterParty = party as CharacterParty;
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < actionData.successRate) {
                ActionSuccess(targetObject);
                GiveAllReward(characterParty);
                if (characterParty.IsFull(NEEDS.FULLNESS)) {
                    EndAction(characterParty, targetObject);
                }
            } else {
                ActionFail(targetObject);
                GiveReward(NEEDS.ENERGY, characterParty);
            }
        }
        
    }
    public override CharacterAction Clone() {
        HuntAction huntAction = new HuntAction();
        SetCommonData(huntAction);
        huntAction.Initialize();
        return huntAction;
    }
    #endregion
}