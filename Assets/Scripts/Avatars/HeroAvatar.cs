using UnityEngine;
using System.Collections;

public class HeroAvatar : CharacterAvatar {

	internal override void NewMove() {
		if(_characters[0].party != null){
			if(_characters[0].party.isInCombat){
				_characters[0].party.SetCurrentFunction (() => NewMove ());
				return;
			}
		}else{
			if(_characters[0].isInCombat){
				_characters[0].SetCurrentFunction (() => NewMove ());
				return;
			}
		}
        CharacterTask currTask = _characters[0].currentTask;
        if (currTask is Expand) {
			if (this.targetLocation.tileLocation.isOccupied && ((Expand)currTask).targetUnoccupiedTile.id == this.targetLocation.tileLocation.id) {
                _characters[0].currentTask.EndTask(TASK_STATUS.FAIL);
                return;
            }
        }
        if (this.path.Count > 0) {
			//RemoveCharactersFromLocation(this.currLocation);
			this.MakeCitizenMove(this.specificLocation.tileLocation, this.path[0]);
        }

	}
}
