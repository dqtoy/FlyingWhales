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
    public override void OnFirstEncounter(CharacterParty party) {
        base.OnFirstEncounter(party);
        StartEncounter(party);
    }
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        //What happens when performing attack
    }
    public override CharacterAction Clone(ObjectState state) {
        AttackAction attackAction = new AttackAction(state);
        SetCommonData(attackAction);
        attackAction.Initialize();
        return attackAction;
    }
    public override bool CanBeDoneBy(CharacterParty party) {
        if (_icharacterObj.iparty.faction != null) {
            if (party.faction.id == _icharacterObj.iparty.faction.id || (_icharacterObj.iparty.attackedByFaction != null && _icharacterObj.iparty.attackedByFaction.id != party.faction.id)) {
                return false;
            }
        }
        return base.CanBeDoneBy(party);
    }
    public override void OnChooseAction(IParty iparty) {
        _icharacterObj.iparty.numOfAttackers++;
        if(_icharacterObj.iparty.attackedByFaction == null) {
            _icharacterObj.iparty.attackedByFaction = iparty.faction;
        }
        base.OnChooseAction(iparty);
    }
    public override void EndAction(CharacterParty party) {
        _icharacterObj.iparty.numOfAttackers--;
        if (_icharacterObj.iparty.numOfAttackers <= 0) {
            _icharacterObj.iparty.numOfAttackers = 0;
            _icharacterObj.iparty.attackedByFaction = null;
        }
        base.EndAction(party);
    }
    public override void SuccessEndAction(CharacterParty party) {
        base.SuccessEndAction(party);
        GiveAllReward(party);
    }
    #endregion
    private void StartEncounter(CharacterParty enemy) {
        enemy.actionData.SetIsHalted(true);
        if(_icharacterObj.iparty is CharacterParty) {
            (_icharacterObj.iparty as CharacterParty).actionData.SetIsHalted(true);
        }

        StartCombatWith(enemy);
    }
    private void StartCombatWith(CharacterParty enemy) {
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        Combat combat = _icharacterObj.iparty.icharacters[0].currentCombat;
        if (combat == null) {
            combat = new Combat();
            combat.AddParty(SIDES.A, enemy);
            combat.AddParty(SIDES.B, _icharacterObj.iparty);
            //MultiThreadPool.Instance.AddToThreadPool(combat);
            Debug.Log("Starting combat between " + enemy.name + " and  " + _icharacterObj.iparty.name);
            combat.CombatSimulation();
        } else {
            if(enemy.icharacters[0].currentCombat != null && enemy.icharacters[0].currentCombat == combat) {
                return;
            }
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(_icharacterObj.iparty.icharacters[0].currentSide);
            combat.AddParty(sideToJoin, enemy);
        }

        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(_icharacterObj.iparty, _icharacterObj.iparty.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        for (int i = 0; i < enemy.icharacters.Count; i++) {
            enemy.icharacters[i].AddHistory(combatLog);
        }
        for (int i = 0; i < _icharacterObj.iparty.icharacters.Count; i++) {
            _icharacterObj.iparty.icharacters[i].AddHistory(combatLog);
        }
    }
    public bool CanBeDoneByTesting(CharacterParty party) {
        if(_icharacterObj.iparty is CharacterParty) {
            if (party.id == _icharacterObj.iparty.id) {
                return false;
            }
        }
        return true;
    }
}
