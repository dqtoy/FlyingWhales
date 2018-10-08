using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisbandPartyAction : CharacterAction {
    public DisbandPartyAction() : base(ACTION_TYPE.DISBAND_PARTY) {
    }

    public override CharacterAction Clone() {
        DisbandPartyAction action = new DisbandPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }

    //public override void PerformAction(CharacterParty party, IObject targetObject) {
    //    base.PerformAction(party, targetObject);
       
    //}
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        CharacterParty charParty = iparty as CharacterParty;
        charParty.DisbandPartyKeepOwner();
        EndAction(charParty, targetObject);
        charParty.actionData.LookForAction();
    }
}
