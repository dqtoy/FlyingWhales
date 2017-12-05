using UnityEngine;
using System.Collections;

public class DefendCityTask : GeneralTask {
	public City targetCity;

	public DefendCityTask(GENERAL_TASKS task, General general, HexTile targetHextile) : base (task, general, targetHextile){
		this.targetCity = targetHextile.city;
	}

	internal override void AssignMoveDate(){
		this.daysBeforeMoving = UnityEngine.Random.Range (20, 31);
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
