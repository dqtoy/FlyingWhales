using UnityEngine;
using System.Collections;
using System.Linq;

public class DrinkBloodState : State {
    public DrinkBloodState(CharacterTask parentTask) : base(parentTask, STATE.DRINK_BLOOD) {
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        Drink();
        return true;
    }
    #endregion

    private void Drink() {
        if (parentTask.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        string chosenAct = TaskManager.Instance.drinkBloodActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
            case "drink":
                //KillCivilianAndDrinkBlood();
                parentTask.EndTask(TASK_STATUS.SUCCESS);
                return;
            case "caught":
                Caught();
                parentTask.EndTask(TASK_STATUS.SUCCESS);
                return;
            case "nothing":
            default:
                break;
        }
    }

    private void Caught() {
        Log caughtLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "DrinkBlood", "caught");
        caughtLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _assignedCharacter.AddHistory(caughtLog);
        (parentTask.targetLocation as BaseLandmark).AddHistory(caughtLog);

		_assignedCharacter.AssignTag(CHARACTER_TAG.CRIMINAL);
    }
}
