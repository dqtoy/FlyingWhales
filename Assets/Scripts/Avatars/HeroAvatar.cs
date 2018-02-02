using UnityEngine;
using System.Collections;

public class HeroAvatar : CharacterAvatar {

	internal override void NewMove() {
        CharacterTask currTask = _characters[0].currentTask;
        if (currTask is Expand) {
            if (this.targetLocation.isOccupied && ((Expand)currTask).targetUnoccupiedTile.id == this.targetLocation.id) {
                _characters[0].currentTask.EndTask(TASK_STATUS.FAIL);
                return;
            }
        }
        if (this.path.Count > 0) {
            this.MakeCitizenMove(this.currLocation, this.path[0]);
        }

	}
}
