using UnityEngine;
using System.Collections;

public class DefendCityTask : GeneralTask {

	public DefendCityTask(GENERAL_TASKS task, General general, City targetCity) : base (task, general, targetCity){
		this.targetCity.hasAssignedDefendGeneral = true;
		this.general.isIdle = true;
//		this.general.targetCity = targetCity;
//		this.general.targetLocation = targetCity.hexTile;
//		this.general.avatar.GetComponent<GeneralAvatar> ().SetHasArrivedState (false);
//		this.general.avatar.GetComponent<GeneralAvatar> ().CreatePath (PATHFINDING_MODE.USE_ROADS_WITH_ALLIES);
		AssignMoveDate ();
	}

	internal override void AssignMoveDate(){
		this.daysBeforeMoving = UnityEngine.Random.Range (30, 46);
		this.moveDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		this.moveDate.AddDays (this.daysBeforeMoving);
		SchedulingManager.Instance.AddEntry (this.moveDate, () => ExecuteTask ());
	}

	#region Overrides
	internal override void Arrived(){
		GetNextTaskOnSched ();
	}
	internal override void DoneTask(){
		base.DoneTask ();
		this.targetCity.hasAssignedDefendGeneral = false;
	}
	#endregion

	private void GetNextTaskOnSched(){
		if(!this.isDone){
			GameDate nextDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			nextDate.AddMonths (2);
			SchedulingManager.Instance.AddEntry (nextDate, () => GetNextTask ());
		}else{
			GetNextTask ();
		}
	}

	private void GetNextTask(){
		this.general.GetTask ();
	}
}
