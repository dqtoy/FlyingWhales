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

    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        (iparty as CharacterParty).actionData.ForceDoAction(iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.IN_PARTY), iparty.icharacterObject);
        if (targetObject is ICharacterObject) {
            (targetObject as ICharacterObject).iparty.AddCharacter(iparty.mainCharacter);
        }
        ActionSuccess(targetObject);
    }

    //public override void PerformAction(CharacterParty party, IObject targetObject) {
    //    base.PerformAction(party, targetObject);
        
    //}
    public override void EndAction(Party party, IObject targetObject) {
        base.EndAction(party, targetObject);
        //party.icon.SetVisualState(false);
    }
    public override bool ShouldGoToTargetObjectOnChoose() {
        return false; //assumes that the party that invited the character is already at the same location
    }
    #endregion
}
