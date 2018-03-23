using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;

public class HuntPrey : CharacterTask {

    private BaseLandmark _target;

	private string hunterName;

	public HuntPrey(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.COMBAT) 
        : base(createdBy, TASK_TYPE.HUNT_PREY, stance, defaultDaysLeft) {
		_states = new System.Collections.Generic.Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.HUNT, new HuntState (this) }
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
			hunterName = _assignedCharacter.name;
			if(_assignedCharacter.party != null){
				hunterName = _assignedCharacter.party.name;
			}
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartHunt ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
    public override void TaskCancel() {
        base.TaskCancel();
        _assignedCharacter.DestroyAvatar();
    }
    public override void TaskFail() {
        base.TaskFail();
        _assignedCharacter.DestroyAvatar();
    }
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0){
			if(character.faction == null){
				return true;
			}else{
				if(location.tileLocation.landmarkOnTile.owner.id != character.faction.id){
					return true;
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

	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
    #endregion

    private void StartHunt() {
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartHunt ());
//			return;
//		}
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "HuntPrey", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _target.AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);

		ChangeStateTo (STATE.HUNT);
    }
}
