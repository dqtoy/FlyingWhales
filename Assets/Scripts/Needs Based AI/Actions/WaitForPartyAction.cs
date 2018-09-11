using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForPartyAction : CharacterAction {

    private IParty waitingParty;

    private int waitCounter;

    public WaitForPartyAction() : base(ACTION_TYPE.WAIT_FOR_PARTY) { }

    #region overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        waitCounter--;
        if (waitCounter <= 0) {
            //done waiting
            StartQuestAction(party);
        } else {
            if (party.mainCharacter.squad.squadMembers.Count == party.mainCharacter.ownParty.icharacters.Count) {
                //if the character's squad is already complete, do not wait
                StartQuestAction(party);
            }
        }
        
    }
    public override CharacterAction Clone() {
        WaitForPartyAction action = new WaitForPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        waitingParty = iparty;
        Messenger.AddListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        GameDate today = GameManager.Instance.Today();
        int deadlineTick = (iparty.owner as Character).GetWorkDeadlineTick();
        //this assumes that the deadline tick is greater than the current tick,
        //if somehow the current tick is greater, the wait counter will become negative and will, thrigger start quest at PerformAction()
        waitCounter = deadlineTick - today.hour;
    }
    //public override void DoneDuration(CharacterParty party, IObject targetObject) {
    //    base.DoneDuration(party, targetObject);
    //    Messenger.RemoveListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
    //    if ((party.owner as ECS.Character).IsSquadLeader()) {
    //        QuestAction questAction = (party.mainCharacter as ECS.Character).currentQuest.GetQuestAction(party.mainCharacter as ECS.Character);
    //        party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
    //    } else {
    //        //means that the character could not join party, start idling at workplace
    //        party.actionData.ForceDoAction(party.characterObject.currentState.GetAction(ACTION_TYPE.IDLE), (party.owner as Character).workplace.landmarkObj);
    //    }
    //}
    #endregion

    private void OnPartyEnteredLandmark(NewParty party, BaseLandmark landmark) {
        if (waitingParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK 
            && landmark.id == waitingParty.specificLocation.id) {
            if (party.mainCharacter is ECS.Character && waitingParty.mainCharacter.squad.squadLeader.id == party.mainCharacter.id) {
                Messenger.RemoveListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
                //the squad leader has arrived, join his party
                (waitingParty as CharacterParty).actionData.ForceDoAction(party.mainCharacter.squad.squadLeader.ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_PARTY), party.mainCharacter.squad.squadLeader.ownParty.icharacterObject);
            }
        }
    }

    private void StartQuestAction(CharacterParty party) {
        Character mainCharacter = party.mainCharacter as Character;
        if (mainCharacter.IsSquadLeader()) {
            QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(party.mainCharacter as ECS.Character);
            party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
        } else {
            //means that the character could not join party, start idling at workplace
            party.actionData.ForceDoAction(party.characterObject.currentState.GetAction(ACTION_TYPE.IDLE), (party.owner as Character).workplace.landmarkObj);
        }
        Messenger.RemoveListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        //QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(mainCharacter);
        //party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
    }
}
