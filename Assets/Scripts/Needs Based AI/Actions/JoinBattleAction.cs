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
    public override void OnFirstEncounter(CharacterParty party) {
        base.OnFirstEncounter(party);
        StartEncounter(party);
    }
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        //What happens when performing join battle
    }
    public override CharacterAction Clone(ObjectState state) {
        JoinBattleAction joinBattleAction = new JoinBattleAction(state);
        SetCommonData(joinBattleAction);
        joinBattleAction.Initialize();
        return joinBattleAction;
    }
    public override bool CanBeDone() {
        if(_characterObj.party.icharacters[0].currentCombat == null) {
            return false;
        }
        return base.CanBeDone();
    }
    public override bool CanBeDoneBy(CharacterParty party) {
        if (party.faction == null || _characterObj.party.faction == null || party.faction.id != _characterObj.party.faction.id) {
            return false;
        }
        return base.CanBeDoneBy(party);
    }
    #endregion
    private void StartEncounter(CharacterParty friend) {
        friend.actionData.SetIsHalted(true);
        FriendWillJoinCombat(friend);
    }
    private void FriendWillJoinCombat(CharacterParty friend) {
        if(_characterObj.party.icharacters[0].currentCombat != null) {
            _characterObj.party.icharacters[0].currentCombat.AddParty(_characterObj.party.icharacters[0].currentSide, friend);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "join_combat");
            combatLog.AddToFillers(friend, friend.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(_characterObj.party.icharacters[0].currentCombat, " joins battle of ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(_characterObj.party, _characterObj.party.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            for (int i = 0; i < friend.icharacters.Count; i++) {
                friend.icharacters[i].AddHistory(combatLog);
            }
            for (int i = 0; i < _characterObj.party.icharacters.Count; i++) {
                _characterObj.party.icharacters[i].AddHistory(combatLog);
            }
        } else {
            CombatManager.Instance.PartyContinuesAction(friend, false);
        }
    }
}
