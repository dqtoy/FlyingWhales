using UnityEngine;
using System.Collections;
using ECS;

public class InfectPsytoxin : CharacterTask {
	public InfectPsytoxin(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) : base(createdBy, TASK_TYPE.INFECT_PSYTOXIN, stance, defaultDaysLeft) {
		_states = new System.Collections.Generic.Dictionary<STATE, State>{
			{STATE.MOVE, new MoveState(this)},
			{STATE.INFECT, new InfectState(this, "Psytoxin")},
		};
	}

	#region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation != null && _targetLocation is BaseLandmark){
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartInfection ());
		}else{
			EndTask(TASK_STATUS.FAIL);
		}
	}
	#endregion

	private void StartInfection(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartInfection ());
//			return;
//		}
		ChangeStateTo (STATE.INFECT);
	}
}
