using UnityEngine;
using System.Collections;

public class VampiricEmbraceState : State {
    public VampiricEmbraceState(CharacterTask parentTask) : base(parentTask, STATE.VAMPIRIC_EMBRACE) {
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        PerformVampiricEmbrace();
        return true;
    }
    #endregion

    private void PerformVampiricEmbrace() {
        ECS.Character targetCharacter = parentTask.specificTarget as ECS.Character;
        string chosenAction = TaskManager.Instance.vampiricEmbraceActions.PickRandomElementGivenWeights();
        if (chosenAction == "turn") {
            Log turnLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "VampiricEmbrace", "turn_success");
            turnLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            turnLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            targetCharacter.AddHistory(turnLog);
            _assignedCharacter.AddHistory(turnLog);
            _targetLandmark.AddHistory(turnLog);
            targetCharacter.AssignTag(CHARACTER_TAG.VAMPIRE);
            parentTask.EndTask(TASK_STATUS.SUCCESS);
        } else if (chosenAction == "caught") {
            Log caughtLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "VampiricEmbrace", "caught");
            caughtLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            caughtLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            targetCharacter.AddHistory(caughtLog);
            _assignedCharacter.AddHistory(caughtLog);
            _targetLandmark.AddHistory(caughtLog);
            if (!_assignedCharacter.HasTag(CHARACTER_TAG.CRIMINAL)) {
                _assignedCharacter.AssignTag(CHARACTER_TAG.CRIMINAL);
            }
            parentTask.EndTask(TASK_STATUS.FAIL);
        }
    }
}
