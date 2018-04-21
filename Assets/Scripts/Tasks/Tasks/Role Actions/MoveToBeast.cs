using UnityEngine;
using System.Collections;
using ECS;
using System.Linq;
using System.Collections.Generic;

public class MoveToBeast : CharacterTask {
    public MoveToBeast(TaskCreator createdBy, int defaultDaysLeft = -1, Quest parentQuest = null, STANCE stance = STANCE.NEUTRAL) 
        : base(createdBy, TASK_TYPE.MOVE_TO_BEAST, stance, defaultDaysLeft, parentQuest) {

        _states = new System.Collections.Generic.Dictionary<STATE, State> {
            { STATE.MOVE, new MoveState (this) }
        };
    }

    #region overrides
    public override CharacterTask CloneTask() {
        MoveToBeast clonedTask = new MoveToBeast(_createdBy, _defaultDaysLeft, _parentQuest, _stance);
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
            _assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS);
        } else {
            EndTask(TASK_STATUS.FAIL);
        }
    }
    public override void PerformTask() {
        if (!CanPerformTask()) {
            return;
        }
        base.PerformTask();
        SuccessTask();
    }
    public override bool CanBeDone(ECS.Character character, ILocation location) {
        return (location is DungeonLandmark);
    }
    public override bool AreConditionsMet(ECS.Character character) {
        //if there are any dungoon landmarks in the characters adjacent regions
        Region regionOfChar = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionOfChar.adjacentRegionsViaRoad.Count; i++) {
            Region adjacentRegion = regionOfChar.adjacentRegionsViaRoad[i];
            List<BaseLandmark> dungeonLandmarks = adjacentRegion.GetLandmarksOfType(BASE_LANDMARK_TYPE.DUNGEON);
            if (dungeonLandmarks.Count > 0) {
                return true;
            }
        }
        return false;
    }
    public override int GetSelectionWeight(Character character) {
        return 30;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        Region regionOfChar = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionOfChar.adjacentRegionsViaRoad.Count; i++) {
            Region adjacentRegion = regionOfChar.adjacentRegionsViaRoad[i];
            List<BaseLandmark> dungeonLandmarks = adjacentRegion.GetLandmarksOfType(BASE_LANDMARK_TYPE.DUNGEON);
            for (int j = 0; j < dungeonLandmarks.Count; j++) {
                BaseLandmark currLandmark = dungeonLandmarks[j];
                int weight = 20; //Each Dungeon landmark in adjacent regions: 20
                if (adjacentRegion.owner == null) {
                    weight += 10;//If Adjacent Region's Settlement is unoccupied: +10 for each Dungeons in that region
                }
                if (currLandmark.charactersAtLocation.Count <= 0) {
                    weight += 100; //If Dungeon does not have any characters: +100
                }
                if (weight > 0) {
                    _landmarkWeights.AddElement(currLandmark, weight);
                }
            }
        }
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0) {
            return _landmarkWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        DoNothing doNothing = new DoNothing(_assignedCharacter, 3);
        //doNothing.SetDaysLeft (3);
        doNothing.OnChooseTask(_assignedCharacter);
    }

}
