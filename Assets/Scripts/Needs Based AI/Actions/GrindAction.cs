using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrindAction : CharacterAction {
    public GrindAction() : base(ACTION_TYPE.GRIND) {
    }

    #region overrides
    public override CharacterAction Clone() {
        GrindAction action = new GrindAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //This is an Action that a character may do to get more powerful. 
        //The character will attempt to find a Monster Group that is at least 15% weaker than him and attack it.
        List<MonsterParty> choices = new List<MonsterParty>();
        for (int i = 0; i < MonsterManager.Instance.allMonsterParties.Count; i++) {
            MonsterParty currParty = MonsterManager.Instance.allMonsterParties[i];
            float powerComparison = Utilities.GetPowerComparison(party, currParty);
            if (powerComparison >= 15f) {
                choices.Add(currParty);
            }
        }
        if (choices.Count > 0) {
            choices = choices.OrderBy(x => x.specificLocation.tileLocation.GetDistanceTo(party.specificLocation.tileLocation)).ToList();
            MonsterParty chosenParty = choices[0];
            party.mainCharacter.AddActionToQueue(chosenParty.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK), chosenParty.icharacterObject);
        } else {
            //IObject obj = null;
            //party.mainCharacter.AddActionToQueue(party.mainCharacter.GetRandomDesperateAction(ref obj), obj);
        }
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        EndAction(party, targetObject);
    }
    #endregion
}
