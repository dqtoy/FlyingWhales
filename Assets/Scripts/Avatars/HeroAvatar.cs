using UnityEngine;
using System.Collections;

public class HeroAvatar : CharacterAvatar {

	internal override void NewMove() {
        Quest currQuest = _characters[0].currentQuest;
        if (currQuest is Expand) {
            if (this.targetLocation.isOccupied && ((Expand)currQuest).targetUnoccupiedTile.id == this.targetLocation.id) {
                _characters[0].currentQuest.EndQuest(QUEST_RESULT.FAIL);
                return;
            }
        }
        if (this.path.Count > 0) {
            this.MakeCitizenMove(this.currLocation, this.path[0]);
        }

	}
}
