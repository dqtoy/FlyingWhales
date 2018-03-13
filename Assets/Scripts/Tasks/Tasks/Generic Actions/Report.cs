using UnityEngine;
using System.Collections;
using ECS;
using System;

public class Report : CharacterTask {

    private ECS.Character _reportTo;
    private Action _onReachLocationAction;

    public Report(TaskCreator createdBy, ECS.Character reportTo, Quest parentQuest = null) : base(createdBy, TASK_TYPE.REPORT, -1, parentQuest) {
        SetStance(STANCE.NEUTRAL);
        _reportTo = reportTo;
        _specificTargetClassification = "character";
        _parentQuest = parentQuest;
        _filters = new TaskFilter[] {
            new MustBeCharacter(reportTo)
        };
        if (parentQuest != null) {
            if (parentQuest is FindLostHeir) {
                _onReachLocationAction = () => FindLostHeirReport();
            }
        }
    }

    #region overrides
    public override CharacterTask CloneTask() {
        Report clonedTask = new Report(_createdBy, _reportTo, _parentQuest);
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
        character.GoToLocation(_reportTo.specificLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, _onReachLocationAction);
    }
    public override void TaskSuccess() {
        base.TaskSuccess();
        //remove the successor tag from the wrong successor then transfer it to the lost heir
        (_parentQuest as FindLostHeir).OnLostHeirFound();
    }
    #endregion

    private void FindLostHeirReport() {
        _assignedCharacter.DestroyAvatar();
        if (_taskStatus == TASK_STATUS.IN_PROGRESS) {
            if (_assignedCharacter.specificLocation.charactersAtLocation.Contains(_reportTo)) {
                //End the find lost heir quest
                //_assignedCharacter.questData.EndQuest(TASK_STATUS.SUCCESS);
                EndTask(TASK_STATUS.SUCCESS);
            } else {
                //go to the location of the character this character is supposed to report to
                _assignedCharacter.GoToLocation(_reportTo.specificLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, _onReachLocationAction);
            }
        }
        
    }
}
