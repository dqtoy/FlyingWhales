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
        if (_icharacterObj.iparty.faction != null) {
            if (character.faction.id == _icharacterObj.iparty.faction.id || (_icharacterObj.iparty.attackedByFaction != null && _icharacterObj.iparty.attackedByFaction.id != character.faction.id)) {
                return false;
            }
        }
        return base.CanBeDoneBy(character);
    }
    public override void OnChooseAction(ICharacter character) {
        _icharacterObj.iparty.numOfAttackers++;
        if(_icharacterObj.iparty.attackedByFaction == null) {
            _icharacterObj.iparty.attackedByFaction = character.faction;
        }
        base.OnChooseAction(character);
    }
    public override void EndAction(Character character) {
        _icharacterObj.iparty.numOfAttackers--;
        if (_icharacterObj.iparty.numOfAttackers <= 0) {
            _icharacterObj.iparty.numOfAttackers = 0;
            _icharacterObj.iparty.attackedByFaction = null;
        }
        base.EndAction(character);
    }
    public override void SuccessEndAction(Character character) {
        base.SuccessEndAction(character);
        GiveAllReward(character);
    }
    #endregion
    private void StartEncounter(Character enemy) {
        enemy.party.SetIsHalted(true);
        if(_icharacterObj.iparty is CharacterParty) {
            (_icharacterObj.iparty as CharacterParty).SetIsHalted(true);
        }

        StartCombatWith(enemy);
    }
    private void StartCombatWith(Character enemy) {
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        Combat combat = _icharacterObj.iparty.currentCombat;
        if (_icharacterObj.iparty.currentCombat == null) {
            combat = new Combat();
            combat.AddCharacter(SIDES.A, enemy);
            combat.AddCharacter(SIDES.B, _icharacterObj.iparty);
            //MultiThreadPool.Instance.AddToThreadPool(combat);
            Debug.Log("Starting combat between " + enemy.name + " and  " + _icharacterObj.iparty.name);
            combat.CombatSimulation();
        } else {
            if(enemy.currentCombat != null && enemy.currentCombat == _icharacterObj.iparty.currentCombat) {
                return;
            }
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(_icharacterObj.iparty.currentSide);
            combat.AddCharacter(sideToJoin, enemy);
        }

        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(_icharacterObj.iparty, _icharacterObj.iparty.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        enemy.AddHistory(combatLog);
        if (_icharacterObj.iparty.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            (_icharacterObj.iparty as Character).AddHistory(combatLog);
        }
    }
    public bool CanBeDoneByTesting(Character character) {
        if(_icharacterObj.iparty.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            if (character.id == _icharacterObj.iparty.id) {
                return false;
            }
        }
        return true;
    }
}
