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
        //What happens when performing join battle
    }
    public override CharacterAction Clone(ObjectState state) {
        JoinBattleAction joinBattleAction = new JoinBattleAction(state);
        SetCommonData(joinBattleAction);
        joinBattleAction.Initialize();
        return joinBattleAction;
    }
    public override bool CanBeDone() {
        if(_characterObj.character.currentCombat == null) {
            return false;
        }
        return base.CanBeDone();
    }
    public override bool CanBeDoneBy(Character character) {
        if (character.faction == null || _characterObj.character.faction == null || character.faction.id != _characterObj.character.faction.id) {
            return false;
        }
        return base.CanBeDoneBy(character);
    }
    #endregion
    private void StartEncounter(Character friend) {
        friend.actionData.SetIsHalted(true);
        FriendWillJoinCombat(friend);
    }
    private void FriendWillJoinCombat(Character friend) {
        if(_characterObj.character.currentCombat != null) {
            _characterObj.character.currentCombat.AddCharacter(_characterObj.character.currentSide, friend);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "join_combat");
            combatLog.AddToFillers(friend, friend.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(_characterObj.character.currentCombat, " joins battle of ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(_characterObj.character, _characterObj.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            friend.AddHistory(combatLog);
            _characterObj.character.AddHistory(combatLog);
        } else {
            CombatManager.Instance.CharacterContinuesAction(friend, false);
        }
    }
}
