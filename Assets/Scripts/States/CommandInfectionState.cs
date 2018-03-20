using UnityEngine;
using System.Collections;
using ECS;

public class CommandInfectionState : State {
	private Character _commandedCharacter;

	public CommandInfectionState(CharacterTask parentTask): base (parentTask, STATE.COMMAND_INFECTION){

	}

	#region Overrides
	public override void OnChooseState (Character character){
		base.OnChooseState (character);
		if(_parentTask is CommandInfection){
			_commandedCharacter = ((CommandInfection)_parentTask).chosenSlyx;
		}
	}
	public override void PerformStateAction (){
		base.PerformStateAction ();
		CommandSlyxToInfectLandmark ();
	}
	protected override void ResetState (){
		base.ResetState ();
		_commandedCharacter = null;
	}
	#endregion

	private void CommandSlyxToInfectLandmark(){
		InfectPsytoxin infectPsytoxin = new InfectPsytoxin (_assignedCharacter, 0);
		infectPsytoxin.SetLocation (_targetLandmark);
		infectPsytoxin.OnChooseTask (_commandedCharacter);
	}
}
