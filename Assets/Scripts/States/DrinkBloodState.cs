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
                KillCivilianAndDrinkBlood();
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

    private void KillCivilianAndDrinkBlood() {
        BaseLandmark targetLocation = (parentTask.targetLocation as BaseLandmark);
        if (targetLocation.civilians > 0) {
            RACE[] races = targetLocation.civiliansByRace.Keys.Where(x => targetLocation.civiliansByRace[x] > 0).ToArray();
            RACE chosenRace = races[UnityEngine.Random.Range(0, races.Length)];
            targetLocation.AdjustCivilians(chosenRace, -1, _assignedCharacter);
            Log killLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "DrinkBlood", "kill");
            killLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            killLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(chosenRace).ToLower(), LOG_IDENTIFIER.OTHER);
            targetLocation.AddHistory(killLog);
            _assignedCharacter.AddHistory(killLog);
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
