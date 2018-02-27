using UnityEngine;
using System.Collections;

public class TakeQuest : CharacterTask {
	public TakeQuest(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.TAKE_QUEST, defaultDaysLeft) {}

    #region overrides
    public override void PerformTask() {
        base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        if (!(_assignedCharacter.specificLocation is Settlement)) {
            //Make sure the character is at a settlement before performing this task
            throw new System.Exception(_assignedCharacter.name + " is not at a settlement!");
        }
		Settlement currSettlement = _assignedCharacter.specificLocation as Settlement;

        WeightedDictionary<Quest> questWeights = new WeightedDictionary<Quest>();
        Quest chosenQuest = null;
        for (int i = 0; i < currSettlement.questBoard.Count; i++) {
            Quest currQuest = currSettlement.questBoard[i];
            questWeights.AddElement(currQuest, 100);
        }
        
        if (questWeights.GetTotalOfWeights() > 0) {
            chosenQuest = questWeights.PickRandomElementGivenWeights();
            Debug.Log(_assignedCharacter.name + " chose to take quest " + chosenQuest.questType.ToString());
        } 
        //else {
        //    chosenTask = new DoNothing(_assignedCharacter);
        //    Debug.Log(_assignedCharacter.name + " could not find any quest to take.");
        //}
        if(chosenQuest != null) {
            _assignedCharacter.SetCurrentQuest(chosenQuest);
        }

        EndTask(TASK_STATUS.SUCCESS);
    }
    public override int GetTaskWeight(ECS.Character character) {
        if (character.specificLocation is Settlement) {
            Settlement settlement = character.specificLocation as Settlement;
            //Take Quest - 400 (0 if no quest available in the current settlement)
            if (settlement.questBoard.Count > 0) {
                return 400;
            }
        }
        return 0;
    }
    #endregion
}
