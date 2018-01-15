using UnityEngine;
using System.Collections;

public class ColonistAvatar : CharacterAvatar {

	internal override void NewMove() {
		if(this.targetLocation.isOccupied){
			_characters [0].currentQuest.EndQuest (QUEST_RESULT.FAIL);

		}else{
			if (this.path.Count > 0) {
				this.MakeCitizenMove(this.currLocation, this.path[0]);
			}
		}

	}
}
