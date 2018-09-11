using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinPartyAction : CharacterAction {
    public JoinPartyAction() : base(ACTION_TYPE.JOIN_PARTY) {
    }

    #region overides
    public override CharacterAction Clone() {
        JoinPartyAction action = new JoinPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }

    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (targetObject is ICharacterObject) {
            (targetObject as ICharacterObject).iparty.AddCharacter(party.mainCharacter);
        }
        ActionSuccess(targetObject);
        party.actionData.ForceDoAction(party.icharacterObject.currentState.GetAction(ACTION_TYPE.IN_PARTY), party.icharacterObject);
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        party.icon.SetVisualState(false);
    }
    #endregion
}
