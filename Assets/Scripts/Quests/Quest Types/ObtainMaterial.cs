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

	public ObtainMaterial(TaskCreator createdBy, BaseLandmark target, MATERIAL materialToObtain) : base(createdBy, QUEST_TYPE.OBTAIN_MATERIAL) {
		_target = target;
		_materialToObtain = materialToObtain;
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
		goToLandmark.onTaskDoAction += goToLandmark.Generic;
		goToLandmark.onTaskActionDone += PerformNextQuestAction;

		Collect collect = new Collect(this);
		collect.InititalizeAction(_materialToCollect);
		collect.onTaskActionDone += this.PerformNextQuestAction;
		collect.onTaskDoAction += collect.ObtainMaterial;

		GoToLocation goBackToSettlement = new GoToLocation(this); //Go to the picked region
		goToLandmark.InititalizeAction(_postedAt);
		goToLandmark.onTaskDoAction += goToLandmark.Generic;
		goToLandmark.onTaskActionDone += TransferMaterialToSettlement;

		_questLine.Enqueue(goToLandmark);
		_questLine.Enqueue(collect);
	}
	#endregion

	private void TransferMaterialToSettlement(){
		AddNewLog("Transfered " + _materialToCollect + " " + Utilities.NormalizeString(_materialToObtain.ToString()) + " to " + _postedAt.landmarkName);
		_postedAt.AdjustMaterial (_materialToObtain, _materialToCollect);
		EndQuest (TASK_STATUS.SUCCESS);
	}
}
