using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class ExploreTile : CharacterTask {

    private BaseLandmark _landmarkToExplore;

    #region getters/setters
    public BaseLandmark landmarkToExplore {
        get { return _landmarkToExplore; }
    }
    #endregion
	public ExploreTile(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.EXPLORE_TILE, defaultDaysLeft) {
		SetStance(STANCE.STEALTHY);
    }
		
    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
            _targetLocation = GetLandmarkTarget(character);
		}

		if (_targetLocation != null) {
			_landmarkToExplore = (BaseLandmark)_targetLocation;
			_assignedCharacter.GoToLocation (_landmarkToExplore, PATHFINDING_MODE.USE_ROADS, () => StartExploration ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override void PerformTask (){
		if(!CanPerformTask()){
			return;
		}
		Explore ();
	}
	public override bool CanBeDone (ECS.Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile is DungeonLandmark){
            return true;
            //if(!character.exploredLandmarks.Contains(location.tileLocation.landmarkOnTile)){
            //if(location.tileLocation.landmarkOnTile.itemsInLandmark.Count > 0){
            //	return true;
            //}
            //}
            //         else{
            //	return true;
            //}
        }
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
        List<Region> regionsToCheck = new List<Region>();
        Region characterRegion = character.specificLocation.tileLocation.region;
        regionsToCheck.Add(characterRegion);
        regionsToCheck.AddRange(characterRegion.adjacentRegionsViaMajorRoad);
        //Check If there are Dungeon Landmarks in current or adjacent regions
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                if(currRegion.landmarks[j] is DungeonLandmark) {
                    return true;
                }
            }
        }
		//for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
		//	BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			//if(CanBeDone(character, landmark)){
			//	return true;
			//}
		//}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 40; //If there are Dungeon Landmarks in current or adjacent regions: 40. Check AreConditionsMet() for requirements.
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        List<Region> regionsToCheck = new List<Region>();
        Region characterRegion = character.specificLocation.tileLocation.region;
        regionsToCheck.Add(characterRegion);
        regionsToCheck.AddRange(characterRegion.adjacentRegionsViaMajorRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark is DungeonLandmark) {
                    int weight = 50; //Each Dungeon Landmark in current and adjacent regions: 50
                    if (character.exploredLandmarks.Contains(currLandmark)) {
                        weight -= 40; //If Dungeon Landmark has been Explored by this character within the past 6 months: -40
                    }
                    if (character.HasRelevanceToQuest(currLandmark)) {
                        weight += 300; //If Dungeon Landmark has relevance with character's current quest: +300
                    }
                    if (weight > 0) {
                        _landmarkWeights.AddElement(currLandmark, weight);
                    }
                }
            }
        }
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
    }
    #endregion

    private void StartExploration(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartExploration ());
			return;
		}
		_assignedCharacter.AddHistory ("Started exploring " + _landmarkToExplore.landmarkName);
	}
	private void Explore(){
        _landmarkToExplore.ExploreLandmark(_assignedCharacter);
		//if( _landmarkToExplore.itemsInLandmark.Count > 0){
		//	int chance = UnityEngine.Random.Range (0, 100);
		//	if(chance < 35){
		//		ECS.Item itemFound = _landmarkToExplore.itemsInLandmark [UnityEngine.Random.Range (0, _landmarkToExplore.itemsInLandmark.Count)];
		//		if(!_assignedCharacter.EquipItem(itemFound)){
		//			_assignedCharacter.PickupItem (itemFound);
		//		}
		//		_landmarkToExplore.RemoveItemInLandmark (itemFound);
		//	}
		//}
		if(_daysLeft == 0){
            _assignedCharacter.AddExploredLandmark(_landmarkToExplore); //After the 5 days is over, the character will record that he has already explored that Landmark.
            End ();
			return;
		}
		ReduceDaysLeft(1);
	}

	private void End(){
		EndTask (TASK_STATUS.SUCCESS);
	}

    #region Logs
    private void LogGoToLocation() {
        AddNewLog("The party travels to " + _landmarkToExplore.location.name);
    }
    #endregion
}
