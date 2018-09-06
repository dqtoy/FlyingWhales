using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForPartyAction : CharacterAction {

    private IParty waitingParty;

    public WaitForPartyAction() : base(ACTION_TYPE.WAIT_FOR_PARTY) { }

    #region overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //waitCounter--;
        //if (waitCounter <= 0) {
        //    //done waiting
        //}
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
        //waitCounter = 6;
        //if the character is a squad member, add listener to when a character is added to his/her location
        //if the character is a squad leader, do nothing.
    }
    public override void DoneDuration(CharacterParty party, IObject targetObject) {
        base.DoneDuration(party, targetObject);
        Messenger.RemoveListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        if ((party.owner as ECS.Character).IsSquadLeader()) {
            QuestAction questAction = (party.mainCharacter as ECS.Character).currentQuest.GetQuestAction(party.mainCharacter as ECS.Character);
            party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
        } else {
            //means that the character could not join party, start idling
        }
    }
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
}
