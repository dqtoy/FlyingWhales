using UnityEngine;
using System.Collections;
using System;

public class AttackState : State {
	Action _wonInCombatAction;

	public AttackState(CharacterTask parentTask, Action wonInCombatAction) : base(parentTask, STATE.ATTACK) {
		_wonInCombatAction = wonInCombatAction;
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
			if(_wonInCombatAction != null){
				source.SetCurrentFunction (() => _wonInCombatAction ());
			}
        }
    }
}
