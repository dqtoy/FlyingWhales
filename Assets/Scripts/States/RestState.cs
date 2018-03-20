using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RestState : State {
	private List<ECS.Character> _charactersToRest;

	public RestState(CharacterTask parentTask): base (parentTask, STATE.REST){

	}
	#region Overrides
	public override void OnChooseState (ECS.Character character){
		base.OnChooseState (character);
		if(_parentTask is Hibernate){
			_charactersToRest = ((Hibernate)_parentTask).charactersToRest;
		}if(_parentTask is Rest){
			_charactersToRest = ((Rest)_parentTask).charactersToRest;
		}
	}
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Rest ();
		return true;
	}
	protected override void ResetState (){
		base.ResetState ();
		_charactersToRest = null;
	}
	#endregion

	private void Rest(){
		for (int i = 0; i < _charactersToRest.Count; i++) {
			ECS.Character currCharacter = _charactersToRest[i];
			currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
		}
	}
}
