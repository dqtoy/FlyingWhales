using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForPartyAction : CharacterAction {

    //private int waitCounter;

    private Character waitingCharacter;

    public WaitForPartyAction() : base(ACTION_TYPE.WAIT_FOR_PARTY) { }

    #region overrides
    //public override void PerformAction(CharacterParty party, IObject targetObject) {
    //    base.PerformAction(party, targetObject);
    //    //waitCounter--;
    //    //if (waitCounter <= 0) {
    //    //    //done waiting
    //    //    StartQuestAction(party);
    //    //} else {
    //        if (party.mainCharacter.squad.squadMembers.Count == party.mainCharacter.ownParty.icharacters.Count) {
    //            //if the character's squad is already complete, do not wait
    //            StartQuestAction(party);
    //        }
    //    //}
    //}
    public override CharacterAction Clone() {
        WaitForPartyAction action = new WaitForPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        waitingCharacter = iparty.owner as Character;
        GameDate today = GameManager.Instance.Today();
        int deadlineTick = (iparty.owner as Character).GetWorkDeadlineTick();
        //this assumes that the deadline tick is greater than the current tick,
        //if somehow the current tick is greater, the wait counter will become negative and will, thrigger start quest at PerformAction()
        this._actionData.duration = deadlineTick - today.hour;
        today.AddDays(_actionData.duration);
        //if (waitingCharacter.IsSquadLeader()) { //only listen for characters joining a party if this character is a squad leader
        //    Messenger.AddListener<ICharacter, Party>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        //}
        Debug.Log(iparty.name + " started waiting for their party mates. Will end wait on [" + today.GetDayAndTicksString() + "]");
    }
    public override void DoneDuration(Party party, IObject targetObject) {
        base.DoneDuration(party, targetObject);
        //done waiting
        StartQuestAction(party);
    }
    public override void EndAction(Party party, IObject targetObject) {
        base.EndAction(party, targetObject);
        //if (waitingCharacter.IsSquadLeader()) {
        //    Messenger.RemoveListener<ICharacter, Party>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        //}
    }
    #endregion

    private void OnCharacterJoinedParty(ICharacter character, Party party) {
        if (waitingCharacter.ownParty.id == party.id) {
            //the party that the character joined is the party of the character that is waiting
            //if (waitingCharacter.squad.squadMembers.Count == waitingCharacter.ownParty.icharacters.Count) {
            //    //if the character's squad is already complete, do not wait
            //    StartQuestAction(waitingCharacter.ownParty);
            //}
        }
    }

    private void StartQuestAction(Party party) {
        Character mainCharacter = party.mainCharacter as Character;
        CharacterParty charParty = party as CharacterParty;
        //if (mainCharacter.IsSquadLeader()) {
        //    QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(party.mainCharacter as ECS.Character);
        //    charParty.actionData.ForceDoAction(questAction);
        //} else {
        //    //means that the character could not join party, start idling at workplace
        //    charParty.actionData.ForceDoAction(charParty.characterObject.currentState.GetAction(ACTION_TYPE.IDLE), (party.owner as Character).workplace.landmarkObj);
        //}
        //Messenger.RemoveListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        //QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(mainCharacter);
        //party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
    }
}
