using UnityEngine;
using System.Collections;

public class FaintedState : State {
    public FaintedState(CharacterTask parentTask) : base(parentTask, STATE.FAINTED) {
        
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        RegainHP();
        return true;
    }
    #endregion

    private void RegainHP() {
        _assignedCharacter.AdjustHP(_assignedCharacter.raceSetting.restRegenAmount);
    }
}
