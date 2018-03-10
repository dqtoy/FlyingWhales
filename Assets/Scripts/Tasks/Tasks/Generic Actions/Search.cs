using UnityEngine;
using System.Collections;
using ECS;
using System;

public class Search : CharacterTask {

    private Action _searchAction;
    private object _searchingFor;

    public Search(TaskCreator createdBy, int defaultDaysLeft, object searchingFor, ILocation targetLocation) : base(createdBy, TASK_TYPE.SEARCH, defaultDaysLeft) {
        SetStance(STANCE.NEUTRAL);
        _targetLocation = targetLocation;
        _searchingFor = searchingFor;
        if (searchingFor is string) {
            if((searchingFor as string).Equals("Heirloom Necklace")) {
                _searchAction = () => SearchForHeirloomNecklace();
            }
        }
    }

    #region overrides
    public override bool CanBeDone(Character character, ILocation location) {
        if (location.charactersAtLocation.Count > 0) {
            return true;
        }
        return base.CanBeDone(character, location);
    }
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        character.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP);
    }
    public override void PerformTask() {
        base.PerformTask();
        _searchAction();
        if (_daysLeft == 0) {
            //EndRecruitment();
            EndSearch();
            return;
        }
        ReduceDaysLeft(1);
    }
    #endregion

    private void SearchForHeirloomNecklace() {
        if (targetLocation is BaseLandmark) {
            (targetLocation as BaseLandmark).AddHistory(_assignedCharacter.name + " is searching for the owner of the Heirloom Necklace!");
        }
        for (int i = 0; i < targetLocation.charactersAtLocation.Count; i++) {
            ECS.Character currCharacter = targetLocation.charactersAtLocation[i].mainCharacter;
            if (currCharacter.HasItem(_searchingFor as string)) {

                //Each day while he is in Search State, if the character with the Heirloom Necklace is in the location then he would successfully perform the action and end the Search State.
            }
        }
    }

    private void EndSearch() {

    }
}
