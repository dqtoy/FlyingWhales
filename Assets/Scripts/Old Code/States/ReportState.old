using UnityEngine;
using System.Collections;

public class ReportState : State {
    public ReportState(CharacterTask parentTask) : base(parentTask, STATE.REPORT) {
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        if (parentTask.parentQuest != null) {
            if (parentTask.parentQuest is FindLostHeir) {
                FindLostHeirReport();
            }
        }
        return true;
    }
    #endregion

    private void FindLostHeirReport() {
        if (parentTask.taskStatus == TASK_STATUS.IN_PROGRESS) {
            if (_assignedCharacter.specificLocation.charactersAtLocation.Contains((parentTask as Report).reportTo)) {
                //End the find lost heir quest
                //_assignedCharacter.questData.EndQuest(TASK_STATUS.SUCCESS);
                parentTask.EndTask(TASK_STATUS.SUCCESS);
            } else {
                //go to the location of the character this character is supposed to report to
                parentTask.ChangeStateTo(STATE.MOVE);
                _assignedCharacter.GoToLocation((parentTask as Report).reportTo.specificLocation, PATHFINDING_MODE.USE_ROADS, () => parentTask.ChangeStateTo(STATE.REPORT));
            }
        }

    }
}
