using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefendCity : GameEvent {
	internal General general;
	internal Battle battle;
	internal City sourceCity;

	internal List<ReinforceCity> reinforcements;

	public DefendCity(int startDay, int startMonth, int startYear, Citizen startedBy, Battle battle) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.DEFEND_CITY;
		this.name = "Defend City";
		this.general = (General)startedBy.assignedRole;
		this.battle = battle;
		this.sourceCity = startedBy.city;
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
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.general.DestroyGO();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
}
