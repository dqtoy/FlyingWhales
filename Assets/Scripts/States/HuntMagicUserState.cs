using UnityEngine;
using System.Collections;

public class HuntMagicUserState : State {

    public HuntMagicUserState(CharacterTask parentTask) : base(parentTask, STATE.HUNT_MAGIC_USER) {

    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        InitiateCombat();
        return true;
    }
    #endregion

    private void InitiateCombat() {
        if (!_assignedCharacter.isInCombat) {
			ECS.Character targetCharacter = _parentTask.specificTarget as ECS.Character;
			ICombatInitializer source = _assignedCharacter;
			ICombatInitializer target = targetCharacter;
			if(_assignedCharacter.party != null){
				source = _assignedCharacter.party;
			}
			if(targetCharacter.party != null){
				target = targetCharacter.party;
			}
			_assignedCharacter.specificLocation.StartCombatBetween(source, target);
        }
    }
}
