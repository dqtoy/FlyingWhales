using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Raze : CharacterTask {

	private BaseLandmark _target;

	public Raze(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.COMBAT) : base(createdBy, TASK_TYPE.RAZE, stance, defaultDaysLeft) {
        _alignments.Add(ACTION_ALIGNMENT.HOSTILE);
        _alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);

		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.RAZE, new RazeState (this) }
		};
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
//		_razingCharacters.Clear ();
//		if(character.party == null){
//			_razingCharacters.Add (character);
//		}else{
//			_razingCharacters.AddRange (character.party.partyMembers);
//		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget(character);
		}
		if (_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
			_target = _targetLocation as BaseLandmark;
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartRaze());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone (Character character, ILocation location){
		//if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0){
		//	if(character.faction == null){
		//		return true;
		//	}else{
  //              if (location.HasHostilitiesWith(character.faction)) {
  //                  return true;
		//		}
		//	}
		//}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.landmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.landmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 20;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        Region regionOfChar = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionOfChar.landmarks.Count; i++) {
            BaseLandmark currLandmark = regionOfChar.landmarks[i];
            int weight = 0;
            if (currLandmark.HasHostilitiesWith(character.faction)) {
                weight += 100; //Landmark is owned by a hostile faction: 100
            }
            //if (currLandmark.civilians > 0) {
            //    weight += 30; //Landmark has civilians: +30
            //}
            if (weight > 0) {
                _landmarkWeights.AddElement(currLandmark, weight);
            }
        }
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
        return null;
    }
	public override void EndTaskSuccess (){
		_currentState.SetIsHalted (false);
		_currentState.PerformStateAction ();
		base.EndTaskSuccess ();
	}
    #endregion

    private void StartRaze(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartRaze ());
//			return;
//		}
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Raze", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startLog.AddToFillers(_target, _target.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        _target.AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);

		ChangeStateTo (STATE.RAZE, true);
        //_target.AddHistory(_assignedCharacter.name + " has started razing " + _target.landmarkName + "!");
        //_target.AddHistory("Started razing " + _target.landmarkName + "!");
    }
}
