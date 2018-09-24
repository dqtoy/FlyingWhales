using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class StalkAction : CharacterAction {

    public StalkAction() : base(ACTION_TYPE.STALK) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void PerformAction(NewParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override IObject GetTargetObject(CharacterParty sourceParty) {
        Character stalker = sourceParty.mainCharacter as Character;
        Stalker stalkerAtt = stalker.GetAttribute(ATTRIBUTE.STALKER) as Stalker;
        return stalkerAtt.stalkee.ownParty.icharacterObject;
    }
    public override CharacterAction Clone() {
        StalkAction action = new StalkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
