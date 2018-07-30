using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ChatAction : CharacterAction {
    private List<Character> _chatters;
    private Character _chatee;
    private WaitingInteractionAction _chateeWaitAction;

    #region getters/setters
    public List<Character> chatters {
        get { return _chatters; }
    }
    public Character chatee {
        get { return _chatee; }
    }
    public WaitingInteractionAction chateeWaitAction {
        get { return _chateeWaitAction; }
    }
    #endregion

    public ChatAction() : base(ACTION_TYPE.CHAT) {
        _actionData.providedSanity = 3f;
        _actionData.duration = 12;
        _chatters = new List<Character>();
        _chatee = null;
        _chateeWaitAction = null;
    }

    #region Overrides
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        Reset();
        if (targetObject != null && targetObject is CharacterObj) {
            CharacterParty targetParty = (targetObject as CharacterObj).party;
            Character targetCharacter = targetParty.mainCharacter as Character;
            WaitingInteractionAction waitingInteractionAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WAITING) as WaitingInteractionAction;
            waitingInteractionAction.SetWaitedCharacter(iparty.mainCharacter as Character);
            waitingInteractionAction.SetWaitedAction(this);

            SetChatee(targetCharacter, waitingInteractionAction);
            //put wait interaction in target queue
            targetCharacter.AddActionToQueue(waitingInteractionAction, null, null, 0);
        }
    }
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        if(targetObject == null) {
            return;
        }
        if(targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty.currentCombat == null) {
                party.icon.SetMovementState(true);
                icharacterObject.iparty.icon.SetMovementState(true);

                if (icharacterObject.iparty is CharacterParty) {
                    CharacterParty targetParty = icharacterObject.iparty as CharacterParty;
                    Character targetCharacter = targetParty.mainCharacter as Character;

                    _chatee.RemoveActionFromQueue(_chateeWaitAction);
                    //Remove wait for interaction action

                    if (targetParty.actionData.currentAction.actionData.actionType != ACTION_TYPE.CHAT) {
                        targetParty.actionData.currentAction.EndAction(targetParty, targetObject);
                        ChatAction actionToAssign = targetParty.mainCharacter.GetIdleOrDesperateAction(_actionData.actionCategory, _actionData.actionType) as ChatAction;
                        targetParty.actionData.AssignAction(actionToAssign, null);
                        actionToAssign.AddChatter(party.mainCharacter as Character);
                    } else {
                        ChatAction currentChatOfTarget = targetParty.actionData.currentAction as ChatAction;
                        currentChatOfTarget.AddChatter(party.mainCharacter as Character);
                    }
                }

                Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChatAction", "start_chat");
                log.AddToFillers(party.mainCharacter, party.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(icharacterObject.iparty.mainCharacter, icharacterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                party.mainCharacter.AddHistory(log);
                icharacterObject.iparty.mainCharacter.AddHistory(log);
            } else {
                //Log
                EndAction(party, targetObject);
                Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChatAction", "combat_instead");
                log.AddToFillers(party.mainCharacter, party.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(icharacterObject.iparty.mainCharacter, icharacterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                party.mainCharacter.AddHistory(log);
                icharacterObject.iparty.mainCharacter.AddHistory(log);

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
        if (party.actionData.isDone) {
            return;
        }
        //Relationship effects
        RemoveChatee(party.mainCharacter as Character);

        if (_chatters.Count <= 0 && _chatee == null) {
            party.icon.SetMovementState(false);
            base.EndAction(party, targetObject);
        }
    }
    public override void PartyPerformingActionChangedState(CharacterParty partyPerformer, IObject targetObject, ObjectState stateThatEnded) {
        base.PartyPerformingActionChangedState(partyPerformer, targetObject, stateThatEnded);
        if (stateThatEnded.stateName == "Alive") {
            //while (_chatters.Count > 0) {
            //    Character chatter = _chatters[0];
            //    if (chatter.party.actionData.currentAction.actionData.actionType == ACTION_TYPE.CHAT) {
            //        ChatAction chatterAction = chatter.party.actionData.currentAction as ChatAction;
            //        chatterAction.EndAction(chatter.party, chatter.party.actionData.currentTargetObject);
            //    }
            //}
            EndAction(partyPerformer, targetObject);
        }
    }
    public override void APartyHasEndedItsState(CharacterParty party, IObject targetObject, CharacterParty partyThatChangedState, ObjectState stateThatEnded) {
        base.APartyHasEndedItsState(party, targetObject, partyThatChangedState, stateThatEnded);
        if (_chatee != null) {
            if (stateThatEnded.stateName == "Alive" && _chatee.party.id == partyThatChangedState.id) {
                EndAction(party, targetObject);
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

    #region Utilities
    private void Reset() {
        _chatters.Clear();
        _chatee = null;
        _chateeWaitAction = null;
    }
    public void RemoveChatter(Character character) {
        _chatters.Remove(character);
    }
    public void AddChatter(Character character) {
        _chatters.Add(character);
    }
    public void SetChatee(Character character, WaitingInteractionAction waitingInteractionAction) {
        _chatee = character;
        _chateeWaitAction = waitingInteractionAction;
    }
    public void RemoveChatee(Character chatter) {
        if (_chatee != null) {
            _chatee.RemoveActionFromQueue(_chateeWaitAction);
            if (_chatee.party.actionData.currentAction.actionType == ACTION_TYPE.CHAT) {
                ChatAction chateeAction = _chatee.party.actionData.currentAction as ChatAction;
                chateeAction.RemoveChatter(chatter);
                if (chateeAction.chatters.Count <= 0 && chateeAction.chatee == null) {
                    _chatee.party.icon.SetMovementState(false);
                    _chatee.party.actionData.EndAction();
                }
            } else if (_chatee.party.actionData.currentAction.actionType == ACTION_TYPE.WAITING) {
                if(_chatee.party.actionData.currentAction == _chateeWaitAction) {
                    _chatee.party.actionData.EndAction();
                }
            }
            SetChatee(null, null);
        }
    }
    #endregion
}
