using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class State {
	protected CharacterTask _parentTask;
	protected STATE _stateType;
	protected string _stateName;
	protected Character _assignedCharacter;
	protected BaseLandmark _targetLandmark;
	protected bool _isHalted;

	#region getters/setters
	public string stateName{
		get { return _stateName; }
	}
	public STATE stateType{
		get { return _stateType; }
	}
	public CharacterTask parentTask{
		get { return _parentTask; }
	}
	public bool isHalted{
		get { return _isHalted; }
	}
	#endregion

	public State(CharacterTask parentTask, STATE stateType){
		_parentTask = parentTask;
		_stateType = stateType;
		_stateName = Utilities.NormalizeStringUpperCaseFirstLetters (_stateType.ToString ());
		_isHalted = false;
	}

	#region Virtuals
	public virtual bool PerformStateAction(){
		return !_isHalted;
	}
	protected virtual void ResetState(){
		_assignedCharacter = null;
		_targetLandmark = null;
	}
	public virtual void OnChooseState(Character character){
		ResetState ();
		_assignedCharacter = character;
		if(_parentTask.targetLocation != null && _parentTask.targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			SetTargetLandmark(_parentTask.targetLocation as BaseLandmark);
		}
	}
	#endregion

	public void SetTargetLandmark(BaseLandmark landmark){
		_targetLandmark = landmark;
	}

	public void SetIsHalted(bool state){
		_isHalted = state;
	}
}
