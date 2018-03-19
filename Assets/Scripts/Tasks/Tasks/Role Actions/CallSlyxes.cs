using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class CallSlyxes : CharacterTask {
	private CraterBeast craterBeast;

	public CallSlyxes(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.CALL_SLYXES, stance, defaultDaysLeft) {
	}

	#region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		craterBeast = (CraterBeast)_assignedCharacter.role;
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();

		craterBeast.CallAllSlyxes ();

		if(_daysLeft == 0){
			EndCall();
			return;
		}
		ReduceDaysLeft (1);
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location == character.specificLocation){
			CraterBeast beast = (CraterBeast)character.role;
			if(beast.numOfSlyxesCanBeCalled > 0){
				return true;
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		if(CanBeDone(character, character.specificLocation)){
			return true;
		}
		return base.AreConditionsMet (character);
	}
	public override int GetSelectionWeight (Character character){
		CraterBeast beast = (CraterBeast)character.role;
		return beast.numOfSlyxesCanBeCalled * 50;
	}
	#endregion

	private void EndCall() {
		EndTask(TASK_STATUS.SUCCESS);
	}

}
