using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackAction : CharacterAction {
    Character _character;
    public AttackAction(ObjectState state) : base(state, ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if(_state.obj.objectType == OBJECT_TYPE.CHARACTER) {
            CharacterObj characterObj = _state.obj as CharacterObj;
            _character = characterObj.character;
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
        _character.actionData.SetIsHalted(true);

        StartCombatWith(enemy);
    }
    private void StartCombatWith(Character enemy) {
        Combat combat = new Combat(_character.specificLocation);
        combat.AddCharacter(SIDES.A, enemy);
        combat.AddCharacter(SIDES.B, _character);
        combat.CombatSimulation();
    }
}
