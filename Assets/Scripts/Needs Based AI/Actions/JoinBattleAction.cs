using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        if(targetObject is CharacterObj) {
            StartEncounter(party, targetObject as CharacterObj);
        }
    }
    public override void PerformAction(Party party, IObject targetObject) {
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
            if (characterObj.party.currentCombat == null) {
                return false;
            }
        }
        return base.CanBeDone(targetObject);
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        if(targetObject is CharacterObj) {
            CharacterObj characterObj = targetObject as CharacterObj;
            if (party.faction == null || characterObj.party.faction == null || party.faction.id != characterObj.party.faction.id) {
                return false;
            }
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    #endregion
    private void StartEncounter(Party friend, CharacterObj characterObj) {
        friend.JoinCombatWith(characterObj.party);
    }
}
