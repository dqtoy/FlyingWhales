using UnityEngine;
using System.Collections;

public class Collect : TaskAction {

	private int _amount;

	public Collect(Quest quest): base (quest){}

	#region overrides
	public override void InititalizeAction(int amount) {
		base.InititalizeAction(amount);
		_amount = amount;
	}
	#endregion

	//This is the DoAction Function in Expand Quest
	internal void Expand(){
		this.actionDoer.currLocation.landmarkOnTile.AdjustReservedPopulation (-_amount);
		((Expand)_task).SetCivilians (_amount);
        _task.AddNewLog(this.actionDoer.name + " takes " + _amount.ToString() + " civilians from " + this.actionDoer.currLocation.landmarkOnTile.landmarkName);
        ActionDone (TASK_ACTION_RESULT.SUCCESS);
	}
}
