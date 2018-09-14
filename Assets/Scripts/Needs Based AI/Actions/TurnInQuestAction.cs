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
        mainCharacter.TurnInQuest();
        EndAction(party, targetObject);
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        if ((party.owner as Character).dailySchedule.currentPhase.phaseType != SCHEDULE_PHASE_TYPE.WORK) {
            //the turn in quest action has reached another phase, disband party after doing this action
            (party.owner as Character).AddActionToQueue(party.characterObject.currentState.GetAction(ACTION_TYPE.DISBAND_PARTY), party.characterObject);
        }
    }
    #endregion
}
