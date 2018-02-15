using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Collect : TaskAction {

	private int _amount;
    private Dictionary<RACE, int> _civilians;


	public Collect(Quest quest): base (quest){}

	#region overrides
	public override void InititalizeAction(Dictionary<RACE, int> civilians) {
		base.InititalizeAction(civilians);
		_civilians = civilians;
	}
	#endregion

	//This is the DoAction Function in Expand Quest
	internal void Expand(){
        //add civilians to party
        ((Expand)_task).assignedParty.AdjustCivilians(_civilians);
        _task.AddNewLog(this.actionDoer.name + " takes " + _civilians.Sum(x => x.Value).ToString() + " civilians from " + this.actionDoer.currLocation.landmarkOnTile.landmarkName);
        ActionDone (TASK_ACTION_RESULT.SUCCESS);
	}

    //This is the DoAction Function for the build structure quest
    internal void BuildStructure() {
        BuildStructure bsQuest = _task as BuildStructure;
        bsQuest.assignedParty.AdjustCivilians(_civilians);
        _task.AddNewLog(this.actionDoer.name + " takes " + _civilians.Sum(x => x.Value).ToString() + " civilians from " + bsQuest.postedAt.landmarkName);
        ActionDone(TASK_ACTION_RESULT.SUCCESS);
    }

	internal void ObtainMaterial(){
		ObtainMaterial obtainMaterial = (ObtainMaterial)_task;
		obtainMaterial.target.ReduceReserveMaterial (obtainMaterial.materialToObtain, _amount);
		if(obtainMaterial.target.materialsInventory[obtainMaterial.materialToObtain].excess > 0){
			int excess = obtainMaterial.target.materialsInventory [obtainMaterial.materialToObtain].excess;
			_amount += excess;
			obtainMaterial.AdjustMaterialToCollect (excess);
			obtainMaterial.target.AdjustMaterial (obtainMaterial.materialToObtain, -excess);
		}
        obtainMaterial.assignedParty.AdjustMaterial(obtainMaterial.materialToObtain, _amount);//Add materials to party inventory
		_task.AddNewLog(this.actionDoer.name + " takes " + _amount.ToString() + " " + Utilities.NormalizeString(obtainMaterial.materialToObtain.ToString()) + " from " + obtainMaterial.target.landmarkName);
		obtainMaterial.target.AddHistory (((this.actionDoer.party != null) ? this.actionDoer.party.name : this.actionDoer.name) + " took " + _amount.ToString () + " " + Utilities.NormalizeString (obtainMaterial.materialToObtain.ToString ()) + ".");
		ActionDone(TASK_ACTION_RESULT.SUCCESS);
	}
}
