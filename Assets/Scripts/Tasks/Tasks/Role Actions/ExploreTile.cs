using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	private BaseLandmark GetLandmarkToExplore(){
		//TODO: Add weights for all landmark to explore and choose one
		return null;
	}
    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		if(_targetLocation == null){
			_landmarkToExplore = GetLandmarkToExplore();
		}else{
			_landmarkToExplore = (BaseLandmark)_targetLocation;
		}
		_assignedCharacter.GoToLocation (_landmarkToExplore, PATHFINDING_MODE.NORMAL, () => StartExploration ());
	}
	public override void PerformTask (){
		base.PerformTask ();
		Explore ();
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
				_landmarkToExplore.itemsInLandmark.Remove (itemFound);
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
    #region Logs
    private void LogGoToLocation() {
        AddNewLog("The party travels to " + _landmarkToExplore.location.name);
    }
    #endregion
}
