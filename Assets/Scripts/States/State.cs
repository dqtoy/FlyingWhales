using UnityEngine;
using System.Collections;

public class State {
	protected CharacterTask _parentTask;
	protected STATE _stateType;
	protected string _stateName;

	public State(CharacterTask parentTask, STATE stateType){
		_parentTask = parentTask;
		_stateType = stateType;
		_stateName = Utilities.NormalizeStringUpperCaseFirstLetters (_stateType.ToString ());
	}

	#region Virtuals
	protected virtual void PerformStateAction(){}
	#endregion
}
