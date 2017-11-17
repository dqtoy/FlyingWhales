using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefendCity : GameEvent {
	internal General general;
	internal Battle battle;
	internal City sourceCity;
	internal Kingdom sourceKingdom;

	internal List<ReinforceCity> reinforcements;

	public DefendCity(int startDay, int startMonth, int startYear, Citizen startedBy, Battle battle) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.DEFEND_CITY;
		this.name = "Defend City";
		this.general = (General)startedBy.assignedRole;
		this.battle = battle;
		this.sourceCity = startedBy.city;
		this.sourceKingdom = this.sourceCity.kingdom;
		this.reinforcements = new List<ReinforceCity> ();
	}
	internal void AddReinforcements(ReinforceCity reinforceCity){
		this.reinforcements.Add (reinforceCity);
		reinforceCity.defendCity = this;
	}
	internal void RemoveReinforcements(ReinforceCity reinforceCity){
		this.reinforcements.Remove (reinforceCity);
	}
	internal void TransferReinforcements(ReinforceCity reinforceCity){
		this.general.AdjustSoldiers (reinforceCity.general.soldiers);
		RemoveReinforcements (reinforceCity);
	}
	internal void ReturnRemainingSoldiers(){
		if(this.isActive){
			if(!this.sourceCity.isDead){
				ReturnSoldiers (this.sourceCity);
			}else{
				if(!this.sourceKingdom.isDead){
					for (int i = 0; i < this.sourceCity.region.connections.Count; i++) {
						if(this.sourceCity.region.connections[i] is Region){
							City city = ((Region)this.sourceCity.region.connections [i]).occupant;
							if(city != null && city.kingdom.id == this.sourceKingdom.id){
								ReturnSoldiers (city);
								break;
							}
						}
					}
				}else{
					this.DoneEvent ();
				}
			}
		}
	}
	private void ReturnSoldiers(City targetCity){
		this.general.isReturning = true;
		this.general.isIdle = false;
		this.general.targetCity = targetCity;
		this.general.targetLocation = targetCity.hexTile;
		this.general.avatar.GetComponent<GeneralAvatar> ().SetHasArrivedState (false);
		this.general.avatar.GetComponent<GeneralAvatar> ().CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
	}
	internal void DropSoldiersAndDisappear(){
		if (this.isActive) {
			if (this.general.location.city != null) {
				this.general.location.city.AdjustSoldiers (this.general.soldiers);
				this.DoneEvent ();
			}
		}
	}
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(citizen.id == this.general.citizen.id){
			if(this.general.targetLocation.city == null || (this.general.targetLocation.city != null && this.general.targetLocation.city.id != this.general.targetCity.id)){
				CancelEvent ();
				return;
			}
			if(this.general.isReturning){
				if(!this.general.targetCity.isDead){
					this.general.targetCity.AdjustSoldiers (this.general.soldiers);
					this.DoneEvent ();
				}else{
					CancelEvent ();
				}
			}
		}
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.general.DestroyGO();
	}
	internal override void CancelEvent (){
		ReturnRemainingSoldiers ();
		base.CancelEvent ();
	}
	#endregion
}
