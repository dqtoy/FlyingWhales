using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackCity : GameEvent {

	internal General general;
	internal Battle battle;
	internal City sourceCity;
	internal City targetCity;

	internal List<ReinforceCity> reinforcements;

	public AttackCity(int startDay, int startMonth, int startYear, Citizen startedBy, Battle battle, City targetCity) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ATTACK_CITY;
		this.name = "Attack City";
		this.general = (General)startedBy.assignedRole;
		this.battle = battle;
		this.sourceCity = startedBy.city;
		this.targetCity = targetCity;
		this.reinforcements = new List<ReinforceCity> ();
	}

	internal void AddReinforcements(ReinforceCity reinforceCity){
		this.reinforcements.Add (reinforceCity);
		reinforceCity.attackCity = this;
	}
	internal void RemoveReinforcements(ReinforceCity reinforceCity){
		this.reinforcements.Remove (reinforceCity);
		if(this.reinforcements.Count <= 0){
			Attack ();
		}
	}
	internal void TransferReinforcements(ReinforceCity reinforceCity){
		this.general.AdjustSoldiers (reinforceCity.general.soldiers);
		RemoveReinforcements (reinforceCity);
	}
	internal void Attack(){
		this.general.isIdle = false;
		this.general.avatar.GetComponent<GeneralAvatar> ().StartMoving ();
		this.battle.Attack ();

	}
	internal void ReturnRemainingSoldiers (){
		
	}
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(citizen.id == this.general.citizen.id){
			if(!this.targetCity.isDead){
				battle.Combat ();
			}else{
				CancelEvent ();
			}
		}
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.general.DestroyGO();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		if(!this.sourceCity.isDead){
			this.sourceCity.AdjustSoldiers (this.general.soldiers);
		}
		this.DoneEvent ();
	}
	#endregion
}
