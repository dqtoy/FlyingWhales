using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class JoinBattleAction : CharacterAction {
    CharacterObj _characterObj;

    #region getters/setters
    public CharacterObj characterObj {
        get { return _characterObj; }
    }
    #endregion
    public JoinBattleAction(ObjectState state) : base(state, ACTION_TYPE.JOIN_BATTLE) {

    }
    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (_state.obj.objectType == OBJECT_TYPE.CHARACTER) {
            _characterObj = _state.obj as CharacterObj;
        }
    }
    public override void OnFirstEncounter(Character character) {
        base.OnFirstEncounter(character);
        StartEncounter(character);
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        //What happens when performing attack
    }
    public override CharacterAction Clone(ObjectState state) {
        AttackAction attackAction = new AttackAction(state);
        SetCommonData(attackAction);
        attackAction.Initialize();
        return attackAction;
    }
    #endregion
    private void StartEncounter(Character enemy) {
        enemy.actionData.SetIsHalted(true);
        _characterObj.character.actionData.SetIsHalted(true);

        JoinCombatWith(enemy);
    }
    private void JoinCombatWith(Character friend) {
        friend.currentCombat.AddCharacter(friend.currentSide, _characterObj.character);
    }
}
