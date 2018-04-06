using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RitualState : State {

	public RitualState(CharacterTask parentTask): base (parentTask, STATE.RITUAL){

	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }

		//TODO: Change this to random ritual effects
		DoMeteorStrike ();
		return true;
	}
	#endregion

	private void DoMeteorStrike(){
		if(_parentTask.taskType == TASK_TYPE.DO_RITUAL){
			DoRitual doRitual = _parentTask as DoRitual;
			MeteorStrikeData meteorStrike = new MeteorStrikeData (doRitual.ritualStones);
			StorylineManager.Instance.AddStoryline (meteorStrike);
		}
	}
}
