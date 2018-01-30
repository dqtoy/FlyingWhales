using UnityEngine;
using System.Collections;

public class TakeQuest : CharacterTask {
    public TakeQuest(TaskCreator createdBy) : base(createdBy, TASK_TYPE.TAKE_QUEST) {}

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if (character.party != null) {
            character.party.SetCurrentTask(this);
        }

        Settlement currSettlement = (Settlement)character.currLocation.landmarkOnTile; //NOTE: Make sure the character is at a settlement before performing this task

        WeightedDictionary<CharacterTask> questWeights = character.role.GetQuestWeights();
        if(questWeights.GetTotalOfWeights() > 0) {
            CharacterTask chosenQuest = questWeights.PickRandomElementGivenWeights();
            chosenQuest.PerformTask(character);
        } else {
            //Do nothing
            DoNothing doNothing = new DoNothing(character);
            doNothing.PerformTask(character);
        }
        

        EndTask(TASK_RESULT.SUCCESS);
    }
    public override void EndTask(TASK_RESULT taskResult) {
        _taskResult = taskResult;
        _isDone = true;
        switch (taskResult) {
            case TASK_RESULT.SUCCESS:
                TaskSuccess();
                break;
            case TASK_RESULT.FAIL:
                TaskFail();
                break;
            case TASK_RESULT.CANCEL:
                TaskCancel();
                break;
            default:
                break;
        }
    }
    #endregion
}
