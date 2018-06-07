using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackAction : CharacterAction {
    CharacterObj _characterObj;

    #region getters/setters
    public CharacterObj characterObj {
        get { return _characterObj; }
    }
    #endregion
    public AttackAction(ObjectState state) : base(state, ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if(_state.obj.objectType == OBJECT_TYPE.CHARACTER) {
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
    public override bool CanBeDoneBy(Character character) {
        if(character.faction.id == _characterObj.character.faction.id) {
            return false;
        }
        return base.CanBeDoneBy(character);
    }
    #endregion
    private void StartEncounter(Character enemy) {
        enemy.actionData.SetIsHalted(true);
        _characterObj.character.actionData.SetIsHalted(true);

        StartCombatWith(enemy);
    }
    private void StartCombatWith(Character enemy) {
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        if (_characterObj.character.currentCombat == null) {
            Combat combat = new Combat(_characterObj.character.specificLocation);
            combat.AddCharacter(SIDES.A, enemy);
            combat.AddCharacter(SIDES.B, _characterObj.character);
            //MultiThreadPool.Instance.AddToThreadPool(combat);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
            combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(_characterObj.character, _characterObj.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            enemy.AddHistory(combatLog);
            _characterObj.character.AddHistory(combatLog);
            Debug.Log("Starting combat between " + enemy.name + " and  " + _characterObj.character.name);
            combat.CombatSimulation();
        } else {
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(_characterObj.character.currentSide);
            _characterObj.character.currentCombat.AddCharacter(sideToJoin, enemy);
        }
       
    }
}
