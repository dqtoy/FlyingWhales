using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class Faint : CharacterTask {
    public Faint(TaskCreator createdBy, STANCE stance = STANCE.NEUTRAL, int defaultDaysLeft = 5, Quest parentQuest = null) 
        : base(createdBy, TASK_TYPE.FAINT, stance, defaultDaysLeft, parentQuest) {
        _states = new Dictionary<STATE, State> {
            { STATE.FAINTED, new FaintedState (this) }
        };
    }

    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        ChangeStateTo(STATE.FAINTED);
    }
    public override void PerformTask() {
        if (!CanPerformTask()) {
            return;
        }
        if (_currentState != null) {
            _currentState.PerformStateAction();
        }
        //if (CharacterRegainedAllHP()) {
        //    EndTaskSuccess();
        //} else {
        if (_daysLeft == 0) {
            EndTaskSuccess();
            return;
        }
        ReduceDaysLeft(1);
        //}
    }
    //private bool CharacterRegainedAllHP() {
    //    return _assignedCharacter.IsHealthFull();
    //}
}
