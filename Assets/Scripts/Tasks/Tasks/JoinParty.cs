using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class JoinParty : CharacterTask {

    private Party _partyToJoin;

    #region getters/setters
    public Party partyToJoin {
        get { return _partyToJoin; }
    }
    #endregion

    public JoinParty(TaskCreator createdBy, Party partyToJoin) 
        : base(createdBy, TASK_TYPE.JOIN_PARTY) {
        _partyToJoin = partyToJoin;
    }

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        _partyToJoin.AddPartyMember(character);
        if(_partyToJoin.currentTask != null && _partyToJoin.currentTask is Quest) {
            (_partyToJoin.currentTask as Quest).OnPartyMemberJoined();
        }
        EndTask(TASK_STATUS.SUCCESS);
        //((Quest)_partyToJoin.currentTask).SetWaitingStatus(true); //wait for the character that will join the party
        //StartGoingToParty();
    }
  //  protected override void ConstructQuestLine() {
  //      base.ConstructQuestLine();

  //      _partyToJoin.AddPartyMemberAsOnTheWay((ECS.Character)_createdBy);

  //      GoToLocation goToLocation = new GoToLocation(this);
  //      goToLocation.InititalizeAction(partyToJoin.partyLeader.currLocation);
		//goToLocation.onTaskActionDone += SuccessQuest;
  //      goToLocation.onTaskDoAction += goToLocation.Generic;

  //      _questLine.Enqueue(goToLocation);
  //  }
  //  protected override void QuestSuccess() {
  //      _partyToJoin.PartyMemberHasArrived((ECS.Character)_createdBy);
  //      _partyToJoin.AddPartyMember((ECS.Character)_createdBy);
  //      if (_partyToJoin.partyMembers.Count < 5) {
  //          ((Quest)_partyToJoin.currentTask).CheckPartyMembers(); //When the character successfully arrives at the party leaders location, check if all the party members are present
  //      }
  //  }
    #endregion

	//private void SuccessTask() {
 //       //_partyToJoin.PartyMemberHasArrived((ECS.Character)_createdBy);
 //       _partyToJoin.AddPartyMember((ECS.Character)_createdBy);
 //       //if (_partyToJoin.partyMembers.Count < 5) {
 //           //((Quest)_partyToJoin.currentTask).CheckPartyMembers(); //When the character successfully arrives at the party leaders location, check if all the party members are present
 //       //}
 //       EndTask(TASK_STATUS.SUCCESS);
	//}

    //private void StartGoingToParty() {
    //    //_partyToJoin.AddPartyMemberAsOnTheWay(_assignedCharacter);

    //    GoToLocation goToLocation = new GoToLocation(this);
    //    goToLocation.InititalizeAction(partyToJoin.specificLocation);
    //    goToLocation.onTaskActionDone += SuccessTask;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;

    //    goToLocation.DoAction(_assignedCharacter);
    //}
}
