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

	public ObtainMaterial(TaskCreator createdBy, MATERIAL materialToObtain) : base(createdBy, QUEST_TYPE.OBTAIN_MATERIAL) {
		_materialToObtain = materialToObtain;
		_target = GetTarget ();
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
		goToLandmark.InititalizeAction(((Settlement)_createdBy));
		goToLandmark.onTaskDoAction += goToLandmark.Generic;
		goToLandmark.onTaskActionDone += TransferMaterialToSettlement;

		_questLine.Enqueue(goToLandmark);
		_questLine.Enqueue(collect);
	}
	#endregion

	private void TransferMaterialToSettlement(){
		AddNewLog ("Transfered " + _materialToCollect + " " + Utilities.NormalizeString (_materialToObtain.ToString ()) + " to " + ((Settlement)_createdBy).landmarkName);
		((Settlement)_createdBy).AdjustMaterial (_materialToObtain, _materialToCollect);
		EndQuest (TASK_STATUS.SUCCESS);
	}

	private BaseLandmark GetTarget(){
		WeightedDictionary<BaseLandmark> targetWeights = new WeightedDictionary<BaseLandmark> ();
		Settlement settlement = (Settlement)_createdBy;
		for (int i = 0; i < settlement.owner.settlements.Count; i++) {
			if(settlement.id == settlement.owner.settlements[i].id){
				for (int j = 0; j < settlement.ownedLandmarks.Count; j++) {
					if(settlement.ownedLandmarks[j].materialsInventory[_materialToObtain].excess > 0){
						targetWeights.AddElement (settlement.ownedLandmarks [j], settlement.ownedLandmarks [j].materialsInventory [_materialToObtain].excess);
					}
				}
			}else{
				if(settlement.owner.settlements[i].materialsInventory[_materialToObtain].excess > 0){
					targetWeights.AddElement (settlement.owner.settlements[i], settlement.owner.settlements[i].materialsInventory [_materialToObtain].excess);
				}
			}
		}
		if(targetWeights.Count > 0){
			return targetWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
