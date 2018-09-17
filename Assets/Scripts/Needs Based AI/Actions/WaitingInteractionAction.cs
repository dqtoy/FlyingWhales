using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class WaitingInteractionAction : CharacterAction {
    private Character _waitedCharacter;
    private System.Action _onWaitedCharacterArrivedAction;
    public GameDate waitUntil { get; private set; }

    public WaitingInteractionAction() : base(ACTION_TYPE.WAITING) {
    }

    #region Overrides
    public override CharacterAction Clone() {
        WaitingInteractionAction action = new WaitingInteractionAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
    }
    #endregion

    #region Utilities
    public void SetWaitedCharacter(Character character) {
        _waitedCharacter = character;
    }
    /*
     This sets what the waiting character should do, when the character
     he/she is waiting for arrives
         */
    public void SetOnWaitedCharacterArrivedAction(System.Action action) {
        _onWaitedCharacterArrivedAction = action;
    }
    public void SetWaitUntil(GameDate endDate) {
        waitUntil = endDate;
    }
    #endregion
}
