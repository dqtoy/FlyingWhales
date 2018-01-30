using UnityEngine;
using System.Collections;
using ECS;

public class DoNothing : CharacterTask {
    public DoNothing(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.DO_NOTHING) {
    }

    private void EndQuestAfterDays() {
        ScheduleTaskEnd(Random.Range(4, 9), TASK_RESULT.SUCCESS); //Do Nothing should only last for a random number of days between 4 days to 8 days
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
