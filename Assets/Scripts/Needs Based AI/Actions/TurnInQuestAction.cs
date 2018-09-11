using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInQuestAction : CharacterAction {
    public TurnInQuestAction() : base(ACTION_TYPE.TURN_IN_QUEST) {}

    #region overrides
    public override CharacterAction Clone() {
        TurnInQuestAction action = new TurnInQuestAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //turn in the quest
        Character mainCharacter = party.owner as Character;
        mainCharacter.RemoveQuest();
        EndAction(party, targetObject);
    }
    #endregion
}
