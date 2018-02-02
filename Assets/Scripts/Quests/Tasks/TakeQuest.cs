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
        CharacterTask chosenTask = null;
        if (questWeights.GetTotalOfWeights() > 0) {
            chosenTask = questWeights.PickRandomElementGivenWeights();
        } else {
            chosenTask = new DoNothing(character);
        }
        character.SetTaskToDoNext(chosenTask);

        EndTask(TASK_STATUS.SUCCESS);
    }
    #endregion
}
