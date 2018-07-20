using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class WaitingInteractionAction : CharacterAction {
    private Character _waitedCharacter;
    private CharacterAction _waitedAction;

    public WaitingInteractionAction() : base(ACTION_TYPE.WAITING) {
    }

    #region Overrides
    public override CharacterAction Clone() {
        WaitingInteractionAction action = new WaitingInteractionAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion

    #region Utilities
    public void SetWaitedCharacter(Character character) {
        _waitedCharacter = character;
    }
    public void SetWaitedAction(CharacterAction action) {
        _waitedAction = action;
    }
    #endregion
}
