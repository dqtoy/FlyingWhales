using UnityEngine;
using System.Collections;

public class TakeQuest : CharacterTask {
    public TakeQuest(TaskCreator createdBy) : base(createdBy, TASK_TYPE.TAKE_QUEST) {}

    #region overrides
    public override void PerformTask() {
        base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }

		Settlement currSettlement = (Settlement)_assignedCharacter.currLocation.landmarkOnTile; //NOTE: Make sure the character is at a settlement before performing this task

//		WeightedDictionary<CharacterTask> questWeights = _assignedCharacter.role.GetQuestWeights();
//        CharacterTask chosenTask = null;
//        if (questWeights.GetTotalOfWeights() > 0) {
//            chosenTask = questWeights.PickRandomElementGivenWeights();
//			Debug.Log(_assignedCharacter.name + " chose to take quest " + chosenTask.taskType.ToString());
//        } else {
//			chosenTask = new DoNothing(_assignedCharacter);
//			Debug.Log(_assignedCharacter.name + " could not find any quest to take.");
//        }
//		_assignedCharacter.SetTaskToDoNext(chosenTask);

        EndTask(TASK_STATUS.SUCCESS);
    }
    #endregion
}
