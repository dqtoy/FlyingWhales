using UnityEngine;
using System.Collections;

public class ReinforceCityTask : GeneralTask {
	
	public General mainGeneral;

	public ReinforceCityTask(GENERAL_TASKS task, General general, City targetCity, General mainGeneral) : base (task, general, targetCity){
		this.mainGeneral = mainGeneral;
	}

	#region Overrides
	internal override void Arrived(){
		if (!this.general.isReturning) {
			ReturnRemainingSoldiers ();
		} else {
			this.general.DropSoldiers ();
			this.general.Death (DEATH_REASONS.BATTLE);
		}
	}
	#endregion
}
