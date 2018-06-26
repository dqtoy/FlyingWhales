using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackAction : CharacterAction {
    private ICharacterObject _icharacterObj;

    #region getters/setters
    public ICharacterObject icharacterObj {
        get { return _icharacterObj; }
    }
    #endregion
    public AttackAction(ObjectState state) : base(state, ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if(_state.obj.objectType == OBJECT_TYPE.CHARACTER || _state.obj.objectType == OBJECT_TYPE.MONSTER) {
            _icharacterObj = _state.obj as ICharacterObject;
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
        if(_icharacterObj.icharacter.faction != null) {
            if (character.faction.id == _icharacterObj.icharacter.faction.id || (_icharacterObj.icharacter.attackedByFaction != null && _icharacterObj.icharacter.attackedByFaction.id != character.faction.id)) {
                return false;
            }
        }
        return base.CanBeDoneBy(character);
    }
    public override void OnChooseAction(ICharacter character) {
        _icharacterObj.icharacter.numOfAttackers++;
        if(_icharacterObj.icharacter.attackedByFaction == null) {
            _icharacterObj.icharacter.attackedByFaction = character.faction;
        }
        base.OnChooseAction(character);
    }
    public override void EndAction(Character character) {
        _icharacterObj.icharacter.numOfAttackers--;
        if (_icharacterObj.icharacter.numOfAttackers <= 0) {
            _icharacterObj.icharacter.numOfAttackers = 0;
            _icharacterObj.icharacter.attackedByFaction = null;
        }
        base.EndAction(character);
    }
    #endregion
    private void StartEncounter(Character enemy) {
        enemy.actionData.SetIsHalted(true);
        if(_icharacterObj.icharacter is Character) {
            (_icharacterObj.icharacter as Character).actionData.SetIsHalted(true);
        }

        StartCombatWith(enemy);
    }
    private void StartCombatWith(Character enemy) {
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        if (_icharacterObj.icharacter.currentCombat == null) {
            Combat combat = new Combat();
            combat.AddCharacter(SIDES.A, enemy);
            combat.AddCharacter(SIDES.B, _icharacterObj.icharacter);
            //MultiThreadPool.Instance.AddToThreadPool(combat);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
            combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(_icharacterObj.icharacter, _icharacterObj.icharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            enemy.AddHistory(combatLog);
            if (_icharacterObj.icharacter is Character) {
                (_icharacterObj.icharacter as Character).AddHistory(combatLog);
            }
            Debug.Log("Starting combat between " + enemy.name + " and  " + _icharacterObj.icharacter.name);
            combat.CombatSimulation();
        } else {
            if(enemy.currentCombat != null && enemy.currentCombat == _icharacterObj.icharacter.currentCombat) {
                return;
            }
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(_icharacterObj.icharacter.currentSide);
            _icharacterObj.icharacter.currentCombat.AddCharacter(sideToJoin, enemy);
        }
       
    }
}
