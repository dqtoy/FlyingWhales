using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PrayAction : CharacterAction {

    public PrayAction(ObjectState state) : base(state, ACTION_TYPE.PRAY) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveAllReward(character);
        if (character.role.IsFull(NEEDS.SANITY)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        PrayAction prayAction = new PrayAction(state);
        SetCommonData(prayAction);
        prayAction.Initialize();
        return prayAction;
    }
    #endregion
}
