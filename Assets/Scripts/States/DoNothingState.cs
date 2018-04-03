using UnityEngine;
using System.Collections;

public class DoNothingState : State {
	public DoNothingState(CharacterTask parentTask): base (parentTask, STATE.DO_NOTHING){

	}

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        if (_assignedCharacter.HasTag(CHARACTER_TAG.BETA)) {
            _assignedCharacter.GetTag(CHARACTER_TAG.BETA).PerformDailyAction();
        }
        return true;
    }
    #endregion
}
