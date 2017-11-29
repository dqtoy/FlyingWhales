using UnityEngine;
using System.Collections;

public class GeneralTask {
	public General general;
	public GENERAL_TASKS task;
	public City targetCity;
	public int daysBeforeMoving;
	public GameDate moveDate;
	public bool isDone;
	public GeneralTask(GENERAL_TASKS task, General general, City targetCity){
		this.general = general;
		this.task = task;
		this.targetCity = targetCity;
		this.isDone = false;
	}

	#region Virtuals
	internal virtual void AssignMoveDate(){
		this.daysBeforeMoving = 0;
		this.moveDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		this.moveDate.AddDays (1);
		SchedulingManager.Instance.AddEntry (this.moveDate, () => ExecuteTask ());
	}
	internal virtual void ExecuteTask(){
		if(general.citizen.isDead || this.isDone){
			return;
		}
		DoTask ();
	}
	internal virtual void DoTask(){
		this.general.isIdle = false;
		this.general.avatar.GetComponent<GeneralAvatar> ().StartMoving ();
	}
	internal virtual void Arrived(){
	}
	internal virtual void DoneTask(){
		this.isDone = true;
	}
	#endregion

	internal void ReturnRemainingSoldiers (){
		if(!this.general.citizen.city.isDead){
			ReturnSoldiers (this.general.citizen.city);
		}else{
			if(!this.general.citizen.city.kingdom.isDead){
				for (int i = 0; i < this.general.citizen.city.region.connections.Count; i++) {
					if(this.general.citizen.city.region.connections[i] is Region){
						City city = ((Region)this.general.citizen.city.region.connections [i]).occupant;
						if(city != null && city.kingdom.id == this.general.citizen.city.kingdom.id && Utilities.HasPath(this.general.location, city.hexTile, PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM, this.general.citizen.city.kingdom)){
							ReturnSoldiers (city);
							return;
						}
					}
				}
				this.general.Death (DEATH_REASONS.BATTLE);
			}else{
				this.general.Death (DEATH_REASONS.BATTLE);
			}
		}
	}
	private void ReturnSoldiers(City targetCity){
		this.general.isReturning = true;
		this.general.targetCity = targetCity;
		this.general.targetLocation = targetCity.hexTile;
		this.general.avatar.GetComponent<GeneralAvatar> ().SetHasArrivedState (false);
		this.general.avatar.GetComponent<GeneralAvatar> ().CreatePath (PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM);
	}
}
