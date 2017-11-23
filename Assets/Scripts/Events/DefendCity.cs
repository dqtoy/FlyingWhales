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
		reinforceCity.battle = this.battle;
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
			if (this.general.location.id == this.sourceCity.hexTile.id) {
				if (!this.sourceCity.isDead) {
					this.sourceCity.AdjustSoldiers (this.general.soldiers);
				}
				this.DoneEvent ();
			} else {
				if (!this.sourceCity.isDead) {
					ReturnSoldiers (this.sourceCity);
				} else {
					if (!this.sourceKingdom.isDead) {
						for (int i = 0; i < this.sourceCity.region.connections.Count; i++) {
							if (this.sourceCity.region.connections [i] is Region) {
								City city = ((Region)this.sourceCity.region.connections [i]).occupant;
								if (city != null && city.kingdom.id == this.sourceKingdom.id && Utilities.HasPath (this.general.location, city.hexTile, PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM, this.sourceKingdom)) {
									ReturnSoldiers (city);
									return;
								}
							}
						}
						this.DoneEvent ();
					} else {
						this.DoneEvent ();
					}
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
//		if(this.general.willDropSoldiersAndDisappear){
//			this.general.DropSoldiersAndDisappear ();
//		}
//		if(citizen.id == this.general.citizen.id){
//			if(this.general.targetLocation.city == null || (this.general.targetLocation.city != null && this.general.targetLocation.city.id != this.general.targetCity.id)){
//				CancelEvent ();
//				return;
//			}
//			if(this.general.willDropSoldiersAndDisappear){
//				this.general.DropSoldiersAndDisappear ();
//			}
//			if(this.general.isReturning){
//				if(!this.general.targetCity.isDead){
//					this.general.targetCity.AdjustSoldiers (this.general.soldiers);
//					this.DoneEvent ();
//				}else{
//					CancelEvent ();
//				}
//			}
//		}
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		CancelAllReinforcements ();
		this.general.citizen.Death(DEATH_REASONS.BATTLE);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		if (!this.sourceCity.isDead) {
			this.sourceCity.AdjustSoldiers (this.general.soldiers);
		}
		this.DoneEvent ();
	}
	#endregion

	private void CancelAllReinforcements(){
		if(this.reinforcements.Count > 0){
			for (int i = 0; i < this.reinforcements.Count; i++) {
				this.reinforcements [i].CancelEvent ();
			}
		}
	}
}
