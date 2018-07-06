using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DrinkAction : CharacterAction {
    public DrinkAction() : base(ACTION_TYPE.DRINK) {

    }
    #region Overrides
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        //Add history log
        for (int i = 0; i < party.icharacters.Count; i++) {
            party.icharacters[i].AssignTag(CHARACTER_TAG.DRUNK);
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if (party.IsFull(NEEDS.FUN)) {
            EndAction(party, targetObject);
        }
    }
    public override CharacterAction Clone() {
        DrinkAction drinkAction = new DrinkAction();
        SetCommonData(drinkAction);
        drinkAction.Initialize();
        return drinkAction;
    }
    #endregion
}
