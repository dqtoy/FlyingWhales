using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObtainMaterial : Quest {

	private BaseLandmark _target;
	private MATERIAL _materialToObtain;
	private int _materialToCollect;

	#region getters/setters
	public BaseLandmark target {
		get { return _target; }
	}
	public MATERIAL materialToObtain {
		get { return _materialToObtain; }
	}
	public int materialToCollect {
		get { return _materialToCollect; }
	}
	#endregion

	public ObtainMaterial(TaskCreator createdBy, MATERIAL materialToObtain, BaseLandmark target) : base(createdBy, QUEST_TYPE.OBTAIN_MATERIAL) {
		_materialToObtain = materialToObtain;
		_target = target;
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction((createdBy as BaseLandmark).owner)
		};
	}
	#region overrides
	public override void OnQuestPosted() {
		base.OnQuestPosted();
		//reserve 5 civilians
		_materialToCollect = _target.materialsInventory[_materialToObtain].excess;
		_target.ReserveMaterial(_materialToObtain, _materialToCollect);
		//TODO: Unreserve material when quest expires
	}
	protected override void ConstructQuestLine() {
		base.ConstructQuestLine();

		GoToLocation goToLandmark = new GoToLocation(this); //Go to the picked region
		goToLandmark.InititalizeAction(_target);
        goToLandmark.SetPathfindingMode(PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP);
		goToLandmark.onTaskDoAction += goToLandmark.Generic;
		goToLandmark.onTaskActionDone += PerformNextQuestAction;

		Collect collect = new Collect(this);
		collect.InititalizeAction(_materialToCollect);
		collect.onTaskActionDone += this.PerformNextQuestAction;
		collect.onTaskDoAction += collect.ObtainMaterial;

		GoToLocation goBackToSettlement = new GoToLocation(this); //Go to the picked region
		goBackToSettlement.InititalizeAction(((Settlement)_createdBy));
        goBackToSettlement.SetPathfindingMode(PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP);
        goBackToSettlement.onTaskDoAction += goBackToSettlement.Generic;
		goBackToSettlement.onTaskActionDone += TransferMaterialToSettlement;

		_questLine.Enqueue(goToLandmark);
		_questLine.Enqueue(collect);
		_questLine.Enqueue(goBackToSettlement);
	}
	#endregion

	internal void AdjustMaterialToCollect(int amount){
		_materialToCollect += amount;
	}
	private void TransferMaterialToSettlement(){
		AddNewLog ("Transfered " + _materialToCollect + " " + Utilities.NormalizeString (_materialToObtain.ToString ()) + " to " + ((Settlement)_createdBy).landmarkName);
		((Settlement)_createdBy).AddHistory (_assignedParty.name + " transfered " + _materialToCollect.ToString () + " " + Utilities.NormalizeString (_materialToObtain.ToString ()) + ".");
		((Settlement)_createdBy).AdjustMaterial (_materialToObtain, _materialToCollect);
        _assignedParty.AdjustMaterial(_materialToObtain, -_materialToCollect); //remove materials from the assigned party
        EndQuest (TASK_STATUS.SUCCESS);
	}
}
