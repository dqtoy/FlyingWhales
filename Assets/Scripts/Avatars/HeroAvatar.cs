using UnityEngine;
using System.Collections;

public class HeroAvatar : CharacterAvatar {

	internal override void NewMove() {
		if(this.targetLocation.isOccupied && ((Expand)_characters [0].currentQuest).targetUnoccupiedTile.id == this.targetLocation.id){
			_characters [0].currentQuest.EndQuest (QUEST_RESULT.FAIL);
		}else{
			if (this.path.Count > 0) {
				this.MakeCitizenMove(this.currLocation, this.path[0]);
			}
		}

	}
}
