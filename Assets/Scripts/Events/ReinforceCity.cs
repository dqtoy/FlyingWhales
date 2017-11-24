using UnityEngine;
using System.Collections;

public class ReinforceCity : GameEvent {
	
	internal General general;
	internal Battle battle;
	internal City sourceCity;
	internal Kingdom sourceKingdom;

	internal AttackCity attackCity;
	internal DefendCity defendCity;

	public ReinforceCity(int startDay, int startMonth, int startYear, Citizen startedBy) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REINFORCE_CITY;
		this.name = "Reinforce City";
		this.general = (General)startedBy.assignedRole;
		this.sourceCity = startedBy.city;
		this.sourceKingdom = this.sourceCity.kingdom;
	}
	internal void ReturnRemainingSoldiers (){
		if(this.isActive){
			if(!this.sourceCity.isDead){
				ReturnSoldiers (this.sourceCity);
			}else{
				if(!this.sourceKingdom.isDead){
					for (int i = 0; i < this.sourceCity.region.connections.Count; i++) {
						if(this.sourceCity.region.connections[i] is Region){
							City city = ((Region)this.sourceCity.region.connections [i]).occupant;
							if(city != null && city.kingdom.id == this.sourceKingdom.id && Utilities.HasPath(this.general.location, city.hexTile, PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM, this.sourceKingdom)){
								ReturnSoldiers (city);
								return;
							}
						}
					}
					this.DoneEvent ();
				}else{
					this.DoneEvent ();
				}
			}
		}
	}
	private void ReturnSoldiers(City targetCity){
		this.general.isReturning = true;
		this.general.targetCity = targetCity;
		this.general.targetLocation = targetCity.hexTile;
		this.general.avatar.GetComponent<GeneralAvatar> ().SetHasArrivedState (false);
		this.general.avatar.GetComponent<GeneralAvatar> ().CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
	}
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(citizen.id == this.general.citizen.id){
			if(this.general.targetLocation.city == null || (this.general.targetLocation.city != null && this.general.targetLocation.city.id != this.general.targetCity.id)){
				CancelEvent ();
				return;
			}
			if(!this.general.isReturning){
				if(attackCity != null){
					if(attackCity.isActive){
						attackCity.TransferReinforcements (this);
					}else{
						CancelEvent ();
					}
				}else if(defendCity != null){
					if (defendCity.isActive) {
						defendCity.TransferReinforcements (this);
					}else{
						CancelEvent ();
					}
				}
			}else{
				this.general.DropSoldiersAndDisappear ();
			}

		}
		this.DoneEvent ();
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.general.citizen.Death(DEATH_REASONS.BATTLE);
	}
	internal override void CancelEvent (){
		if(attackCity != null){
			attackCity.RemoveReinforcements (this, true);
		}else if(defendCity != null){
			defendCity.RemoveReinforcements (this);
		}
		ReturnRemainingSoldiers ();
	}
	#endregion
}
