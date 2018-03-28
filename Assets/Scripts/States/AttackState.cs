using UnityEngine;
using System.Collections;
using System;

public class AttackState : State {
	Action _wonInCombatAction;
	ECS.Character _targetCharacter;

	public AttackState(CharacterTask parentTask, Action wonInCombatAction) : base(parentTask, STATE.ATTACK) {
		_wonInCombatAction = wonInCombatAction;
		_targetCharacter = _parentTask.specificTarget as ECS.Character;
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        InitiateCombat();
        return true;
    }
    #endregion

    private void InitiateCombat() {
        if (!_assignedCharacter.isInCombat && !_targetCharacter.isInCombat) {
			ICombatInitializer source = _assignedCharacter;
			ICombatInitializer target = _targetCharacter;
			if(_assignedCharacter.party != null){
				source = _assignedCharacter.party;
			}
			if(_targetCharacter.party != null){
				target = _targetCharacter.party;
			}
			_assignedCharacter.specificLocation.StartCombatBetween(source, target);
			if(_wonInCombatAction != null){
				source.SetCurrentFunction (() => _wonInCombatAction ());
			}
        }
    }
}
