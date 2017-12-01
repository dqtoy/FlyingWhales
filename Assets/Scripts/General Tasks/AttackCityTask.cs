using UnityEngine;
using System.Collections;

public class AttackCityTask : GeneralTask {

	public HexTile rallyPoint;

	public AttackCityTask(GENERAL_TASKS task, General general, City targetCity) : base (task, general, targetCity){
//		this.general.isIdle = true;
//		this.general.targetCity = targetCity;
//		this.general.targetLocation = targetCity.hexTile;
//		this.general.avatar.GetComponent<GeneralAvatar> ().SetHasArrivedState (false);
//		this.general.avatar.GetComponent<GeneralAvatar> ().CreatePath (PATHFINDING_MODE.USE_ROADS_WITH_ALLIES);
		AssignMoveDate ();
	}

	internal override void AssignMoveDate(){
		this.daysBeforeMoving = UnityEngine.Random.Range (30, 41);
		this.moveDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		this.moveDate.AddDays (this.daysBeforeMoving);
		SchedulingManager.Instance.AddEntry (this.moveDate, () => ExecuteTask ());
	}

	#region Overrides
	internal override void Arrived(){
		this.general.GetTask ();
	}
	#endregion
}
