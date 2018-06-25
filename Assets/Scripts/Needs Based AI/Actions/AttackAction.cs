using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackAction : CharacterAction {
    private ICharacterObject _characterObj;

    #region getters/setters
    public ICharacterObject characterObj {
        get { return _characterObj; }
    }
    #endregion
    public AttackAction(ObjectState state) : base(state, ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if(_state.obj.objectType == OBJECT_TYPE.CHARACTER || _state.obj.objectType == OBJECT_TYPE.MONSTER) {
            _characterObj = _state.obj as ICharacterObject;
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
        if(_characterObj.icharacter.faction != null) {
            if (character.faction.id == _characterObj.icharacter.faction.id) {
                return false;
            }
        }
        return base.CanBeDoneBy(character);
    }
    #endregion
    private void StartEncounter(Character enemy) {
        enemy.actionData.SetIsHalted(true);
        if(_characterObj.icharacter is Character) {
            (_characterObj.icharacter as Character).actionData.SetIsHalted(true);
        }

        StartCombatWith(enemy);
    }
    private void StartCombatWith(Character enemy) {
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        if (_characterObj.icharacter.currentCombat == null) {
            Combat combat = new Combat();
            combat.AddCharacter(SIDES.A, enemy);
            combat.AddCharacter(SIDES.B, _characterObj.icharacter);
            //MultiThreadPool.Instance.AddToThreadPool(combat);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
            combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(_characterObj.icharacter, _characterObj.icharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            enemy.AddHistory(combatLog);
            if (_characterObj.icharacter is Character) {
                (_characterObj.icharacter as Character).AddHistory(combatLog);
            }
            Debug.Log("Starting combat between " + enemy.name + " and  " + _characterObj.icharacter.name);
            combat.CombatSimulation();
        } else {
            if(enemy.currentCombat != null && enemy.currentCombat == _characterObj.icharacter.currentCombat) {
                return;
            }
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(_characterObj.icharacter.currentSide);
            _characterObj.icharacter.currentCombat.AddCharacter(sideToJoin, enemy);
        }
       
    }
}
