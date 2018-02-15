using UnityEngine;
using System.Collections;

public class HeroAvatar : CharacterAvatar {

	internal override void NewMove() {
		ICombatInitializer combatInitializer = _characters[0];
		if(_characters[0].party != null){
			combatInitializer = _characters [0].party;
		}
		if(combatInitializer.isInCombat){
			combatInitializer.SetCurrentFunction (() => NewMove ());
			return;
		}
        CharacterTask currTask = _characters[0].currentTask;
        if (currTask is Expand) {
			if (this.targetLocation.tileLocation.isOccupied && ((Expand)currTask).targetUnoccupiedTile.id == this.targetLocation.tileLocation.id) {
                _characters[0].currentTask.EndTask(TASK_STATUS.FAIL);
                return;
            }
        }
        if (this.path.Count > 0) {
            this.MakeCitizenMove(this.currLocation.tileLocation, this.path[0]);
        }

	}
}
