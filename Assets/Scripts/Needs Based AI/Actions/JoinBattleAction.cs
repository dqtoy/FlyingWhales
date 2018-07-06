using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class JoinBattleAction : CharacterAction {
    //CharacterObj _characterObj;

    //#region getters/setters
    //public CharacterObj characterObj {
    //    get { return _characterObj; }
    //}
    //#endregion
    public JoinBattleAction() : base(ACTION_TYPE.JOIN_BATTLE) {

    }
    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
    //    if (_state.obj.objectType == OBJECT_TYPE.CHARACTER) {
    //        _characterObj = _state.obj as CharacterObj;
    //    }
    //}
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        if(targetObject is CharacterObj) {
            StartEncounter(party, targetObject as CharacterObj);
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        //What happens when performing join battle
    }
    public override CharacterAction Clone() {
        JoinBattleAction joinBattleAction = new JoinBattleAction();
        SetCommonData(joinBattleAction);
        joinBattleAction.Initialize();
        return joinBattleAction;
    }
    public override bool CanBeDone(IObject targetObject) {
        if(targetObject is CharacterObj) {
            CharacterObj characterObj = targetObject as CharacterObj;
            if (characterObj.party.icharacters[0].currentCombat == null) {
                return false;
            }
        }
        return base.CanBeDone(targetObject);
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if(targetObject is CharacterObj) {
            CharacterObj characterObj = targetObject as CharacterObj;
            if (party.faction == null || characterObj.party.faction == null || party.faction.id != characterObj.party.faction.id) {
                return false;
            }
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    #endregion
    private void StartEncounter(CharacterParty friend, CharacterObj characterObj) {
        friend.actionData.SetIsHalted(true);
        FriendWillJoinCombat(friend, characterObj);
    }
    private void FriendWillJoinCombat(CharacterParty friend, CharacterObj characterObj) {
        if(characterObj.party.icharacters[0].currentCombat != null) {
            characterObj.party.icharacters[0].currentCombat.AddParty(characterObj.party.icharacters[0].currentSide, friend);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "join_combat");
            combatLog.AddToFillers(friend, friend.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(characterObj.party.icharacters[0].currentCombat, " joins battle of ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(characterObj.party, characterObj.party.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            for (int i = 0; i < friend.icharacters.Count; i++) {
                friend.icharacters[i].AddHistory(combatLog);
            }
            for (int i = 0; i < characterObj.party.icharacters.Count; i++) {
                characterObj.party.icharacters[i].AddHistory(combatLog);
            }
        } else {
            CombatManager.Instance.PartyContinuesAction(friend, false);
        }
    }
}
