using UnityEngine;
using System.Collections;

public class ReinforceCity : GameEvent {
	
	internal General general;
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
			if(attackCity != null){
				attackCity.TransferReinforcements (this);
			}else if(defendCity != null){
				defendCity.TransferReinforcements (this);
			}
		}
		this.DoneEvent ();
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
