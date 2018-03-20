using UnityEngine;
using System.Collections;

public class HuntMagicUserState : State {

    public HuntMagicUserState(CharacterTask parentTask) : base(parentTask, STATE.HUNT_MAGIC_USER) {

    }

    #region overrides
    public override void PerformStateAction() {
        base.PerformStateAction();
        InitiateCombat();
    }
    #endregion

    private void InitiateCombat() {
        if (!_assignedCharacter.isInCombat) {
            _assignedCharacter.specificLocation.StartCombatBetween(_assignedCharacter, _parentTask.specificTarget as ECS.Character);
        }
    }
}
