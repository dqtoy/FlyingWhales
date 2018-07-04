using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DrinkAction : CharacterAction {
    public DrinkAction(ObjectState state) : base(state, ACTION_TYPE.DRINK) {

    }
    #region Overrides
    public override void OnFirstEncounter(CharacterParty party) {
        base.OnFirstEncounter(party);
        //Add history log
        for (int i = 0; i < party.icharacters.Count; i++) {
            party.icharacters[i].AssignTag(CHARACTER_TAG.DRUNK);
        }
    }
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
        if (party.IsFull(NEEDS.FUN)) {
            EndAction(party);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        DrinkAction drinkAction = new DrinkAction(state);
        SetCommonData(drinkAction);
        drinkAction.Initialize();
        return drinkAction;
    }
    #endregion
}
