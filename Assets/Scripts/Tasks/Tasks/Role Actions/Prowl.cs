using UnityEngine;
using System.Collections;
using ECS;

public class Prowl : CharacterTask {
    public Prowl(TaskCreator createdBy, int defaultDaysLeft = -1, Quest parentQuest = null, STANCE stance = STANCE.COMBAT) 
        : base(createdBy, TASK_TYPE.PROWL, stance, defaultDaysLeft, parentQuest) {

        _states = new System.Collections.Generic.Dictionary<STATE, State> {
            { STATE.MOVE, new MoveState (this) },
            { STATE.PROWL, new ProwlState (this) }
        };
    }

    #region overrides
    public override CharacterTask CloneTask() {
        MoveTo clonedTask = new MoveTo(_createdBy, _defaultDaysLeft, _parentQuest, _stance);
        clonedTask.SetForGameOnly(_forGameOnly);
        clonedTask.SetForPlayerOnly(_forPlayerOnly);
        return clonedTask;
    }
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
        if (_assignedCharacter == null) {
            return;
        }
        if (_targetLocation == null) {
            _targetLocation = GetLandmarkTarget(character);
        }
        if (_targetLocation != null) {
            //Debug.Log(_assignedCharacter.name + " goes to " + _targetLocation.locationName);
            ChangeStateTo(STATE.MOVE);
            _assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartProwl());
        } else {
            EndTask(TASK_STATUS.FAIL);
        }
    }
    public override bool CanBeDone(ECS.Character character, ILocation location) {
        return (location is BaseLandmark);
    }
    public override bool AreConditionsMet(ECS.Character character) {
        Region regionOfChar = character.specificLocation.tileLocation.region;
        return regionOfChar.allLandmarks.Count > 0; //if there are any landmarks in the region the character is in, return true.
    }
    public override int GetSelectionWeight(Character character) {
        return 40;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        Region regionOfChar = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionOfChar.allLandmarks.Count; i++) {
            BaseLandmark currLandmark = regionOfChar.allLandmarks[i];
            int weight = 20; //Each landmark in the region: +20
            if (currLandmark.owner == null) {
                weight += 50; //Landmark is not owned by any Faction: +50
            }
            if (!(currLandmark is Settlement)) {
                weight += 50; //Landmark is not a settlement type: +50
            }
            if (weight > 0) {
                _landmarkWeights.AddElement(currLandmark, weight);
            }
        }
        if (_landmarkWeights.GetTotalOfWeights() > 0) {
            return _landmarkWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
    #endregion

    private void StartProwl() {
        ChangeStateTo(STATE.PROWL);
        SetDefaultDaysLeft(15); //A character will go to the selected landmark and be in Prowl State for up to 15 days
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Prowl", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startLog.AddToFillers(_targetLocation, _targetLocation.locationName, LOG_IDENTIFIER.LANDMARK_1);
        _assignedCharacter.AddHistory(startLog);
        if (_targetLocation is BaseLandmark) {
            (_targetLocation as BaseLandmark).AddHistory(startLog);
        }
        
    }
}
