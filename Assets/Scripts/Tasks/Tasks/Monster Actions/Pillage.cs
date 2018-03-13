using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Pillage : CharacterTask {
    private BaseLandmark _target;

	private string pillagerName;

	public Pillage(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.PILLAGE, defaultDaysLeft) {
		SetStance (STANCE.COMBAT);
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark ();
		}
		_target = (BaseLandmark)_targetLocation;
		pillagerName = _assignedCharacter.name;
		if(_assignedCharacter.party != null){
			pillagerName = _assignedCharacter.party.name;
		}
		_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartPillage ());
//        TriggerSaveLandmarkQuest();
    }
    public override void PerformTask() {
        base.PerformTask();
        //GoToTargetLocation();
        DoPillage();
        
    }
    public override void TaskCancel() {
        base.TaskCancel();
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
//		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
    }
    public override void TaskFail() {
        base.TaskFail();
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
//		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
    }

	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.itemsInLandmark.Count > 0){
			if (location.tileLocation.landmarkOnTile is Settlement || location.tileLocation.landmarkOnTile is ResourceLandmark) {
				if (character.faction == null || location.tileLocation.landmarkOnTile.owner == null) {
					return true;
				} else {
					if (location.tileLocation.landmarkOnTile.owner.id != character.faction.id) {
						return true;
					}
				}
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
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        DoPillage();
    //    }
    //}
    #endregion

    private void StartPillage() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartPillage ());
			return;
		}

		_target.AddHistory(pillagerName + " has started pillaging " + _target.landmarkName + "!");
		_assignedCharacter.AddHistory("Started pillaging " + _target.landmarkName + "!");
    }

    private void DoPillage() {
        if (this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        PILLAGE_ACTION chosenAct = TaskManager.Instance.pillageActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
			case PILLAGE_ACTION.OBTAIN_ITEM:
				ObtainItem ();
                break;
            case PILLAGE_ACTION.END:
                End();
                break;
			case PILLAGE_ACTION.CIVILIAN_DIES:
				CivilianDies();
				break;
            case PILLAGE_ACTION.NOTHING:
            default:
                break;
        }
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft (1);
    }
	private void ObtainItem(){
		if(_target.itemsInLandmark.Count > 0){
			Item chosenItem = _target.itemsInLandmark [UnityEngine.Random.Range (0, _target.itemsInLandmark.Count)];
			if(!_assignedCharacter.EquipItem(chosenItem)){
				_assignedCharacter.PickupItem (chosenItem);
			}
			_target.itemsInLandmark.Remove (chosenItem);
		}
	}
	private void CivilianDies(){
//		int civilians = _target.civilians;
		if(_target.civilians > 0){
			RACE[] races = _target.civiliansByRace.Keys.Where(x => _target.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_target.AdjustCivilians (chosenRace, -1);
			_target.AddHistory (pillagerName + " killed a/an " + Utilities.GetNormalizedSingularRace(chosenRace).ToLower() + " civilian while pillaging!");
			_assignedCharacter.AddHistory ("Killed a/an " + Utilities.GetNormalizedSingularRace(chosenRace).ToLower() + " civilian while pillaging!");

		}
	}
	private void TriggerSaveLandmarkQuest(){
		if(_target.location.region.centerOfMass.landmarkOnTile.isOccupied && !_target.location.region.centerOfMass.landmarkOnTile.AlreadyHasQuestOfType(QUEST_TYPE.SAVE_LANDMARK, _target)){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.SaveALandmark (_target);
		}
	}

	private void End(){
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        //SetCanDoDailyAction(false);
//        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
		EndTask(TASK_STATUS.SUCCESS);
	}
	private BaseLandmark GetTargetLandmark() {
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
//				if(_assignedCharacter.faction == null || landmark.owner == null){
//					_landmarkWeights.AddElement (landmark, 100);
//				}else{
//					if(_assignedCharacter.faction.id != landmark.owner.id){
//						_landmarkWeights.AddElement (landmark, 100);
//					}
//				}
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
