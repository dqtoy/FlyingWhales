using UnityEngine;
using System.Collections;
using ECS;
using System;
using System.Collections.Generic;

public class Report : CharacterTask {

    private ECS.Character _reportTo;

    #region getters/setters
    public ECS.Character reportTo {
        get { return _reportTo; }
    }
    #endregion

    public Report(TaskCreator createdBy, ECS.Character reportTo, Quest parentQuest = null, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.REPORT, stance, -1, parentQuest) {
        _reportTo = reportTo;
        _specificTargetClassification = "character";
        _parentQuest = parentQuest;
        _filters = new TaskFilter[] {
            new MustBeCharacter(reportTo)
        };
        if (parentQuest != null) {
            if (parentQuest is FindLostHeir) {
                _states = new Dictionary<STATE, State>() {
                    { STATE.MOVE, new MoveState(this) },
                    { STATE.REPORT, new ReportState(this) }
                };
            }
        }
    }

    #region overrides
    public override CharacterTask CloneTask() {
		Report clonedTask = new Report(_createdBy, _reportTo, _parentQuest, _stance);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
        return clonedTask;
    }
    public override bool CanBeDone(Character character, ILocation location) {
        if (location.charactersAtLocation.Contains(_reportTo)) {
            return true; //the chieftain that this task needs to report to is at the location, this action can be done.
        }
        return base.CanBeDone(character, location);
    }
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        ChangeStateTo(STATE.MOVE);
        character.GoToLocation(_reportTo.specificLocation, PATHFINDING_MODE.USE_ROADS, () => ChangeStateTo(STATE.REPORT));
    }
    public override void TaskSuccess() {
        base.TaskSuccess();
        //remove the successor tag from the wrong successor then transfer it to the lost heir
        (_parentQuest as FindLostHeir).OnLostHeirFound();
    }
    public override int GetSelectionWeight(Character character) {
        return 100;
    }
    #endregion

    #region Logs
    public override string GetArriveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", this.GetType().ToString(), "arrive_action");
        arriveLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        return Utilities.LogReplacer(arriveLog);
    }
    public override string GetLeaveActionString() {
        Log leaveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", this.GetType().ToString(), "leave_action");
        leaveLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        return Utilities.LogReplacer(leaveLog);
    }
    #endregion
}
