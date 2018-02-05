using UnityEngine;
using System.Collections;

public class HeroAvatar : CharacterAvatar {

	internal override void NewMove() {
        CharacterTask currTask = _characters[0].currentTask;
        if (currTask is Expand) {
            if (this.targetTile.isOccupied && ((Expand)currTask).targetUnoccupiedTile.id == this.targetTile.id) {
                _characters[0].currentTask.EndTask(TASK_STATUS.FAIL);
                return;
            }
        }
        if (this.path.Count > 0) {
            this.MakeCitizenMove(this.currTile, this.path[0]);
        }

	}
}
