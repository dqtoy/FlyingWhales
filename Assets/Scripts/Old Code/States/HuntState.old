using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HuntState : State {

	public HuntState(CharacterTask parentTask): base (parentTask, STATE.HUNT){

	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Hunt ();
		return true;
	}
	#endregion

	private void Hunt(){
		HUNT_ACTION chosenAct = TaskManager.Instance.huntActions.PickRandomElementGivenWeights();
		switch (chosenAct) {
		case HUNT_ACTION.EAT:
			//EatCivilian();
			break;
		case HUNT_ACTION.END:
			parentTask.EndTaskSuccess();
			break;
		case HUNT_ACTION.NOTHING:
			break;
		default:
			break;
		}
	}
}
