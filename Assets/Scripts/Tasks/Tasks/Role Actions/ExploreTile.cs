using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploreTile : CharacterTask {

    private BaseLandmark _landmarkToExplore;
	private ECS.Character _character;
	private int _daysLeft;

    #region getters/setters
    public BaseLandmark landmarkToExplore {
        get { return _landmarkToExplore; }
    }
    #endregion
    public ExploreTile(TaskCreator createdBy) : base(createdBy, TASK_TYPE.EXPLORE_TILE) {
		_character = (ECS.Character)createdBy;
    }

	private BaseLandmark GetLandmarkToExplore(){
		//TODO: Add weights for all landmark to explore and choose one
		return null;
	}
    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		_daysLeft = 5;
		if(_targetLocation == null){
			_landmarkToExplore = GetLandmarkToExplore();
		}else{
			_landmarkToExplore = (BaseLandmark)_targetLocation;
		}
	}
	public override void PerformTask (){
		base.PerformTask ();
		_assignedCharacter.GoToLocation (_landmarkToExplore, PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP, () => StartExploration ());
	}
    #endregion

	private void StartExploration(){
		_character.AddHistory ("Started exploring " + _landmarkToExplore.landmarkName);
		Explore ();
	}
	private void Explore(){
		if(_character.isInCombat){
			_character.SetCurrentFunction (() => Explore ());
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 35){
			ECS.Item itemFound = _landmarkToExplore.itemsInLandmark [UnityEngine.Random.Range (0, _landmarkToExplore.itemsInLandmark.Count)];
			if(!_character.EquipItem(itemFound)){
				_character.PickupItem (itemFound);
			}
			_landmarkToExplore.itemsInLandmark.Remove (itemFound);
		}
		if(_daysLeft != 0){
			ScheduleExploration ();
		}else{
			return;
		}
		_daysLeft--;
	}
	private void ScheduleExploration(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (1);
		SchedulingManager.Instance.AddEntry (newSched, () => Explore ());
	}

    #region Logs
    private void LogGoToLocation() {
        AddNewLog("The party travels to " + _landmarkToExplore.location.name);
    }
    #endregion
}
