using UnityEngine;
using System.Collections;
using ECS;
using System;

[Serializable]
public class Search : CharacterTask {

    private Action _searchAction;
    private object _searchingFor;

    public Search(TaskCreator createdBy, int defaultDaysLeft, object searchingFor, ILocation targetLocation, Quest parentQuest = null) : base(createdBy, TASK_TYPE.SEARCH, defaultDaysLeft, parentQuest) {
        SetStance(STANCE.NEUTRAL);
        _targetLocation = targetLocation;
        _searchingFor = searchingFor;
        if (searchingFor is string) {
            if((searchingFor as string).Equals("Heirloom Necklace")) {
                _searchAction = () => SearchForHeirloomNecklace();
                _alignments.Add(ACTION_ALIGNMENT.LAWFUL);
            }
        }
    }

    #region overrides
    public override CharacterTask CloneTask() {
        Search clonedTask = new Search(_createdBy, _defaultDaysLeft, _searchingFor, _targetLocation, _parentQuest);
        return clonedTask;
    }
    public override bool CanBeDone(Character character, ILocation location) {
        return true;
    }
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_targetLocation == null) {
            WeightedDictionary<BaseLandmark> landmarkWeights = GetLandmarkTargetWeights(character);
            if (landmarkWeights.GetTotalOfWeights() > 0) {
                _targetLocation = landmarkWeights.PickRandomElementGivenWeights();
            } else {
                EndTask(TASK_STATUS.FAIL); //the character could not search anywhere, fail this task
                return;
            }
        }
		_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, () => _assignedCharacter.DestroyAvatar());
    }
    public override bool AreConditionsMet(Character character) {
        //check if there are any landmarks in region with characters
        Region regionLocation = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
            BaseLandmark currLandmark = regionLocation.allLandmarks[i];
            if(currLandmark.charactersAtLocation.Count > 0) {
                return true;
            }
        }
        return base.AreConditionsMet(character);
    }
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();
        _searchAction();
        if (_daysLeft == 0) {
            //EndRecruitment();
            EndTask(TASK_STATUS.FAIL);
            return;
        }
        ReduceDaysLeft(1);
    }
    public override int GetSelectionWeight(ECS.Character character) {
        if (_parentQuest is FindLostHeir) {
            return 80;
        }
        return 0;
    }
    #endregion

    #region Find Lost Heir
    private void SearchForHeirloomNecklace() {
        if (targetLocation is BaseLandmark) {
            (targetLocation as BaseLandmark).AddHistory(_assignedCharacter.name + " is searching for the owner of the Heirloom Necklace!");
        }
        for (int i = 0; i < targetLocation.charactersAtLocation.Count; i++) {
            ECS.Character currCharacter = targetLocation.charactersAtLocation[i].mainCharacter;
            if (currCharacter.HasItem(_searchingFor as string)) {
                //Each day while he is in Search State, if the character with the Heirloom Necklace is in the location then he would successfully perform the action and end the Search State.
                EndTask(TASK_STATUS.SUCCESS);
                //_assignedCharacter.questData.AdvanceToNextPhase();
            }
        }
    }
    private WeightedDictionary<BaseLandmark> GetLandmarkWeights(ECS.Character character) {
        WeightedDictionary<BaseLandmark> landmarkWeights = new WeightedDictionary<BaseLandmark>();
        Region regionLocation = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
            BaseLandmark currLandmark = regionLocation.allLandmarks[i];
            int weight = 0;
            weight += currLandmark.charactersAtLocation.Count * 20;//For each character in a landmark in the current region: +20
            if (currLandmark.HasHostilitiesWith(character.faction)) {
                weight -= 50;//If landmark has hostile characters: -50
            }
            //If this character has already Searched in the landmark within the past 6 months: -60
            if (weight > 0) {
                landmarkWeights.AddElement(currLandmark, weight);
            }
        }
        return landmarkWeights;
    }
    #endregion


}
