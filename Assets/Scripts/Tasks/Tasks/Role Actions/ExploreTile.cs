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
			_landmarkToExplore = GetTargetLandmark();
		}else{
			_landmarkToExplore = (BaseLandmark)_targetLocation;
		}
		_assignedCharacter.GoToLocation (_landmarkToExplore, PATHFINDING_MODE.USE_ROADS, () => StartExploration ());
	}
	public override void PerformTask (){
		base.PerformTask ();
		Explore ();
	}
	public override bool CanBeDone (ECS.Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile is DungeonLandmark){
			if(character.exploredLandmarks.ContainsKey(location.tileLocation.landmarkOnTile.id)){
				if(character.exploredLandmarks[location.tileLocation.landmarkOnTile.id].itemsInLandmark.Count > 0){
					return true;
				}
			}else{
				return true;
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
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
		if(_isHalted){
			return;
		}
		if( _landmarkToExplore.itemsInLandmark.Count > 0){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 35){
				ECS.Item itemFound = _landmarkToExplore.itemsInLandmark [UnityEngine.Random.Range (0, _landmarkToExplore.itemsInLandmark.Count)];
				if(!_assignedCharacter.EquipItem(itemFound)){
					_assignedCharacter.PickupItem (itemFound);
				}
				_landmarkToExplore.RemoveItemInLandmark(itemFound);
			}
		}
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft(1);
	}
	private void ScheduleExploration(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (1);
		SchedulingManager.Instance.AddEntry (newSched, () => Explore ());
	}

	private void End(){
		EndTask (TASK_STATUS.SUCCESS);
	}

	private BaseLandmark GetTargetLandmark(){
		//TODO: Add weights for all landmark the can give the character an item that his current quest needs
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if (CanBeDone (_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
			}
//			if(landmark is DungeonLandmark){
//				if(_assignedCharacter.exploredLandmarks.ContainsKey(landmark.id)){
//					if(landmark.itemsInLandmark.Count > 0){
//						_landmarkWeights.AddElement (landmark, 100);
//					}
//				}else{
//					_landmarkWeights.AddElement (landmark, 100);
//				}
//			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}

    #region Logs
    private void LogGoToLocation() {
        AddNewLog("The party travels to " + _landmarkToExplore.location.name);
    }
    #endregion
}
