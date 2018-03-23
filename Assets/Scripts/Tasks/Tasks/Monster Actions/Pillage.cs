using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Pillage : CharacterTask {
    private BaseLandmark _target;

	private string pillagerName;

	public Pillage(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.COMBAT) 
        : base(createdBy, TASK_TYPE.PILLAGE, stance, defaultDaysLeft) {

		_states = new System.Collections.Generic.Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.PILLAGE, new PillageState (this) }
		};
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget (character);
		}
		if(_targetLocation != null && _targetLocation is BaseLandmark){
			_target = (BaseLandmark)_targetLocation;
			pillagerName = _assignedCharacter.name;
			if(_assignedCharacter.party != null){
				pillagerName = _assignedCharacter.party.name;
			}
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartPillage ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
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

	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
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
    #endregion

    private void StartPillage() {
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartPillage ());
//			return;
//		}

        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Pillage", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _target.AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);

		ChangeStateTo (STATE.PILLAGE);
    }
}
