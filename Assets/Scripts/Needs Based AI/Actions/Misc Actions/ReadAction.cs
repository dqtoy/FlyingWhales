using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class ReadAction : CharacterAction {

    public ReadAction() : base(ACTION_TYPE.READ) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        ReadAction action = new ReadAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override int GetMiscActionWeight(Character character) {
        int weight = base.GetMiscActionWeight(character);
        if (character.HasAttribute(ATTRIBUTE.BOOKWORM)) {
            //if has bookworm attribute, add more weight
            weight += 50;
        }
        return weight;
    }
    #endregion
}
