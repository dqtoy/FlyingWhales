using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatAction : CharacterAction {

    public ChatAction() : base(ACTION_TYPE.CHAT) {
        _actionData.providedSanity = 3f;

        _actionData.duration = 12;
    }

    #region Overrides
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        if(targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if(icharacterObject.iparty.currentCombat == null) {
                if (icharacterObject.iparty is CharacterParty) {
                    CharacterParty characterParty = icharacterObject.iparty as CharacterParty;
                    characterParty.actionData.SetIsHalted(true);
                }
                Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChatAction", "start_chat");
                log.AddToFillers(party.mainCharacter, party.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(icharacterObject.iparty.mainCharacter, icharacterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                party.mainCharacter.AddHistory(log);
                icharacterObject.iparty.mainCharacter.AddHistory(log);
            } else {
                //Log
                Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChatAction", "combat_instead");
                log.AddToFillers(party.mainCharacter, party.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(icharacterObject.iparty.mainCharacter, icharacterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                party.mainCharacter.AddHistory(log);
                icharacterObject.iparty.mainCharacter.AddHistory(log);

                EndAction(party, targetObject);
                party.JoinCombatWith(icharacterObject.iparty);
            }
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        if (targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty is CharacterParty) {
                CharacterParty characterParty = icharacterObject.iparty as CharacterParty;
                if (characterParty.actionData.isHalted) {
                    characterParty.actionData.SetIsHalted(false);
                }
            }
        }
    }
    public override CharacterAction Clone() {
        ChatAction action = new ChatAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
