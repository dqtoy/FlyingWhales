using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackAction : CharacterAction {
    //private ICharacterObject _icharacterObj;
    private bool _hasAttacked;

    #region getters/setters
    public bool hasAttacked {
        get { return _hasAttacked; }
    }
    #endregion
    public AttackAction() : base(ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
        //if(_state.obj.objectType == OBJECT_TYPE.CHARACTER || _state.obj.objectType == OBJECT_TYPE.MONSTER) {
        //    _icharacterObj = _state.obj as ICharacterObject;
        //}
    //}
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        //if(targetObject is ICharacterObject) {
        //    StartEncounter(party, targetObject as ICharacterObject);
        //}
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        //What happens when performing attack
    }
    public override CharacterAction Clone() {
        AttackAction attackAction = new AttackAction();
        SetCommonData(attackAction);
        attackAction.Initialize();
        return attackAction;
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        if(targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty.faction != null) {
                if (party.faction.id == icharacterObject.iparty.faction.id || (icharacterObject.iparty.attackedByFaction != null && icharacterObject.iparty.attackedByFaction.id != party.faction.id)) {
                    return false;
                }
            }
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        //if(targetObject is ICharacterObject) {
        //    ICharacterObject icharacterObject = targetObject as ICharacterObject;
        //    icharacterObject.iparty.numOfAttackers++;
        //    if (icharacterObject.iparty.attackedByFaction == null) {
        //        icharacterObject.iparty.attackedByFaction = iparty.faction;
        //    }
        //}
        base.OnChooseAction(iparty, targetObject);
    }
    public override void EndAction(Party party, IObject targetObject) {
        //if (targetObject is ICharacterObject) {
        //    ICharacterObject icharacterObject = targetObject as ICharacterObject;
        //    icharacterObject.iparty.numOfAttackers--;
        //    if (icharacterObject.iparty.numOfAttackers <= 0) {
        //        icharacterObject.iparty.numOfAttackers = 0;
        //        icharacterObject.iparty.attackedByFaction = null;
        //    }
        //}
        base.EndAction(party, targetObject);
    }
    public override void SuccessEndAction(Party party) {
        base.SuccessEndAction(party);
        //if(party is CharacterParty) {
        //    GiveAllReward(party as CharacterParty);
        //}
    }
    #endregion
    private void StartEncounter(Party enemy, ICharacterObject icharacterObject) {
        Combat combat = icharacterObject.iparty.CreateCombatWith(enemy);
        combat.Fight();

    }

    public bool CanBeDoneByTesting(CharacterParty party, ICharacterObject icharacterObject) {
        if(icharacterObject.iparty is CharacterParty) {
            if (party.id == icharacterObject.iparty.id) {
                return false;
            }
        }
        return true;
    }
}
