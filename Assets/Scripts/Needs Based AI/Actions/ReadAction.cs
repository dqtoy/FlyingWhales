using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadAction : CharacterAction {
    public ReadAction() : base(ACTION_TYPE.READ) { }

    #region Overrides
    public override int GetMiscActionWeight(CharacterParty party, IObject targetObject) {
        if (party.owner.homeStructure != null && party.owner.homeStructure == targetObject) {
            if ((party.owner as ECS.Character).HasTag(CHARACTER_TAG.BOOKWORM)) {
                return base.GetMiscActionWeight(party, targetObject) + 100; //if has bookworm attribute add more weight.
            }
            return base.GetMiscActionWeight(party, targetObject);
        }
        return 0;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        ReadAction action = new ReadAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
