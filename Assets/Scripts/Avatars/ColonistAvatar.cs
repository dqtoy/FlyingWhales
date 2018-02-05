using UnityEngine;
using System.Collections;

public class ColonistAvatar : CharacterAvatar {

	internal override void NewMove() {
		if(this.targetTile.isOccupied && ((Expand)_characters [0].currentTask).targetUnoccupiedTile.id == this.targetTile.id){
			_characters [0].currentTask.EndTask (TASK_STATUS.FAIL);
		}else{
			if (this.path.Count > 0) {
				this.MakeCitizenMove(this.currTile, this.path[0]);
			}
		}

	}
}
