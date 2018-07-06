using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackAction : CharacterAction {
    //private ICharacterObject _icharacterObj;

    //#region getters/setters
    //public ICharacterObject icharacterObj {
    //    get { return _icharacterObj; }
    //}
    //#endregion
    public AttackAction() : base(ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
        //if(_state.obj.objectType == OBJECT_TYPE.CHARACTER || _state.obj.objectType == OBJECT_TYPE.MONSTER) {
        //    _icharacterObj = _state.obj as ICharacterObject;
        //}
    //}
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        if(targetObject is ICharacterObject) {
            StartEncounter(party, targetObject as ICharacterObject);
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
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
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
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
    public override void OnChooseAction(IParty iparty, IObject targetObject) {
        if(targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            icharacterObject.iparty.numOfAttackers++;
            if (icharacterObject.iparty.attackedByFaction == null) {
                icharacterObject.iparty.attackedByFaction = iparty.faction;
            }
        }
        base.OnChooseAction(iparty, targetObject);
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        if (targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            icharacterObject.iparty.numOfAttackers--;
            if (icharacterObject.iparty.numOfAttackers <= 0) {
                icharacterObject.iparty.numOfAttackers = 0;
                icharacterObject.iparty.attackedByFaction = null;
            }
        }
        base.EndAction(party, targetObject);
    }
    public override void SuccessEndAction(CharacterParty party) {
        base.SuccessEndAction(party);
        GiveAllReward(party);
    }
    #endregion
    private void StartEncounter(CharacterParty enemy, ICharacterObject icharacterObject) {
        enemy.actionData.SetIsHalted(true);
        if(icharacterObject.iparty is CharacterParty) {
            (icharacterObject.iparty as CharacterParty).actionData.SetIsHalted(true);
        }

        StartCombatWith(enemy, icharacterObject);
    }
    private void StartCombatWith(CharacterParty enemy, ICharacterObject icharacterObject) {
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        Combat combat = icharacterObject.iparty.icharacters[0].currentCombat;
        if (combat == null) {
            combat = new Combat();
            combat.AddParty(SIDES.A, enemy);
            combat.AddParty(SIDES.B, icharacterObject.iparty);
            //MultiThreadPool.Instance.AddToThreadPool(combat);
            Debug.Log("Starting combat between " + enemy.name + " and  " + icharacterObject.iparty.name);
            combat.CombatSimulation();
        } else {
            if(enemy.icharacters[0].currentCombat != null && enemy.icharacters[0].currentCombat == combat) {
                return;
            }
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(icharacterObject.iparty.icharacters[0].currentSide);
            combat.AddParty(sideToJoin, enemy);
        }

        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(icharacterObject.iparty, icharacterObject.iparty.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        for (int i = 0; i < enemy.icharacters.Count; i++) {
            enemy.icharacters[i].AddHistory(combatLog);
        }
        for (int i = 0; i < icharacterObject.iparty.icharacters.Count; i++) {
            icharacterObject.iparty.icharacters[i].AddHistory(combatLog);
        }
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
