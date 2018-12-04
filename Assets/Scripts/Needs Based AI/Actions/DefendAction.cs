using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DefendAction : CharacterAction {
    public DefendAction() : base(ACTION_TYPE.DEFEND) {

    }


    #region Overrides
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        party.SetIsDefending(true);
        //DefendTheLand(party, targetObject);
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            CharacterParty characterParty = party as CharacterParty;
            GiveAllReward(characterParty);
            DefendTheLand(characterParty, targetObject);
        }
    }
    public override IObject GetTargetObject(CharacterParty sourceParty) {
        return null;
    }
    public override void EndAction(Party party, IObject targetObject) {
        base.EndAction(party, targetObject);
        party.SetIsDefending(false);
    }
    public override CharacterAction Clone() {
        DefendAction action = new DefendAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion

    private void CheckCombatResults(CharacterParty defenderParty, IObject targetObject, Combat combat) {
        if(defenderParty.owner.currentSide == combat.winningSide && !defenderParty.isDead) {
            DefendTheLand(defenderParty, targetObject);
        } else {
            if (!defenderParty.isDead) {
                EndAction(defenderParty, targetObject);
            }
        }
    }

    private void DefendTheLand(CharacterParty defenderParty, IObject targetObject) {
        bool engagedInCombat = false;
        for (int i = 0; i < targetObject.objectLocation.charactersAtLocation.Count; i++) {
            Party newParty = targetObject.objectLocation.charactersAtLocation[i];
            if (newParty.isAttacking) {
                //Combat combat = defenderParty.StartCombatWith(newParty, () => CheckCombatResults(defenderParty, targetObject, combat));
                //combat.AddAfterCombatAction(() => CheckCombatResults(defenderParty, targetObject, combat));
                engagedInCombat = true;
                break;
            }
        }
        if (!engagedInCombat) {
            EndAction(defenderParty, targetObject);
        }
    }
}
