using UnityEngine;
using System.Collections;

public class AttackCityTask : GeneralTask {

	public HexTile rallyPoint;
	public City targetCity;

	public AttackCityTask(GENERAL_TASKS task, General general, HexTile targetHextile) : base (task, general, targetHextile){
		this.targetCity = targetHextile.city;
		Messenger.AddListener<City> ("CityHasDied", CityHasDied);
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
	internal override void DoneTask (){
		base.DoneTask ();
		Messenger.RemoveListener<City> ("CityHasDied", CityHasDied);
	}
	#endregion

	private void CityHasDied(City city){
		if(city.id == this.targetCity.id){
//			DoneTask ();
			this.general.GetTask ();
		}
	}
}
