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
        GameDate today = GameManager.Instance.Today();
        if (IsWaitedCharacterHere(iparty) || today.IsSameDate(waitUntil)) {
            EndAction(iparty as CharacterParty, targetObject);
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        GameDate today = GameManager.Instance.Today();
        if (IsWaitedCharacterHere(party) || today.IsSameDate(waitUntil)) {
            EndAction(party, targetObject);
        }
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
    private bool IsWaitedCharacterHere(NewParty party) {
        for (int i = 0; i < party.specificLocation.charactersAtLocation.Count; i++) {
            NewParty currParty = party.specificLocation.charactersAtLocation[i];
            if (currParty.icharacters.Contains(_waitedCharacter)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
