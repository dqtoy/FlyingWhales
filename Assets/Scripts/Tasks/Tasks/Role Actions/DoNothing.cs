using UnityEngine;
using System.Collections;
using ECS;
using System;

public class DoNothing : CharacterTask {

    private Action endAction;
    private GameDate endDate;

    public DoNothing(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.DO_NOTHING) {
    }

    private void EndQuestAfterDays() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(UnityEngine.Random.Range(4, 9));
        endDate = dueDate;
        endAction = () => EndTask(TASK_STATUS.SUCCESS);
        SchedulingManager.Instance.AddEntry(dueDate, () => endAction());
        //ScheduleTaskEnd(Random.Range(4, 9), TASK_RESULT.SUCCESS); //Do Nothing should only last for a random number of days between 4 days to 8 days
    }

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if(character.party != null) {
            character.party.SetCurrentTask(this);
        }
        EndQuestAfterDays();
    }
    public override void TaskCancel() {
        //Unschedule task end!
        SchedulingManager.Instance.RemoveSpecificEntry(endDate, endAction);
		if(_assignedCharacter.faction != null){
			_assignedCharacter.DetermineAction ();
		}
    }
    //public override void AcceptQuest(ECS.Character partyLeader) {
    //    _isAccepted = true;
    //    partyLeader.SetCurrentTask(this);
    //    if(partyLeader.party != null) {
    //        partyLeader.party.SetCurrentTask(this);
    //    }
    //    this.SetWaitingStatus(false);
    //    if (onQuestAccepted != null) {
    //        onQuestAccepted();
    //    }
    //}
    //internal override void EndQuest(TASK_RESULT result) {
    //    if (!_isDone) {
    //        _questResult = result;
    //        _isDone = true;
    //        _createdBy.RemoveQuest(this);
    //        ((ECS.Character)_createdBy).DetermineAction();
    //    }
    //}
    //internal override void QuestCancel() {
    //    _isDone = true;
    //    _createdBy.RemoveQuest(this);
    //    _questResult = TASK_RESULT.SUCCESS;
    //}
    #endregion
}
