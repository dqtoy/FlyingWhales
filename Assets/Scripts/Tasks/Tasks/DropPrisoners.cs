using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropPrisoners : CharacterTask {

	private GameDate endDate;
	private Region region;
	private List<ECS.Character> _prisoners;

	public DropPrisoners(TaskCreator createdBy) 
		: base(createdBy, TASK_TYPE.DROP_PRISONERS) {
	}

	#region overrides
	public override void PerformTask(ECS.Character character) {
		base.PerformTask(character);
		character.SetCurrentTask(this);
		HexTile currLocation = null;
		if(character.party != null) {
			character.party.SetCurrentTask(this);
			region = character.party.currLocation.region;
			_prisoners = character.party.prisoners;
			currLocation = character.party.currLocation;
		}else{
			region = character.currLocation.region;
			_prisoners = character.prisoners;
			currLocation = character.currLocation;
		}
		if(currLocation.id == region.centerOfMass.id){
			Drop ();
		}else{
			GoToTile ();	
		}
	}
	public override void TaskCancel() {
		//Unschedule task end!
		if(_assignedCharacter.faction != null){
			_assignedCharacter.DetermineAction ();
		}
	}
	#endregion

	private void GoToTile(){
		GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		goToLocation.InititalizeAction(region.centerOfMass);
		goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP);
		goToLocation.onTaskActionDone += Drop;
		goToLocation.onTaskDoAction += goToLocation.Generic;

		goToLocation.DoAction(_assignedCharacter);
	}
	private void Drop(){
		if(region.centerOfMass.landmarkOnTile.owner != null){
			for (int i = 0; i < _prisoners.Count; i++) {
				if(_prisoners[i].faction == null || region.centerOfMass.landmarkOnTile.owner.id != _prisoners[i].faction.id){
					region.centerOfMass.landmarkOnTile.AddHistory ("Dropped prisoner " + _prisoners [i].name);
					_prisoners [i].TransferPrisoner(region.centerOfMass.landmarkOnTile);
					i--;
				}
			}
			EndTask (TASK_STATUS.SUCCESS);
		}else{
			EndTask (TASK_STATUS.CANCEL);
		}

	}
}
