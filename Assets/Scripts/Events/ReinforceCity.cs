using UnityEngine;
using System.Collections;

public class ReinforceCity : GameEvent {
	
	internal General general;
	internal AttackCity attackCity;
	internal DefendCity defendCity;

	public ReinforceCity(int startDay, int startMonth, int startYear, Citizen startedBy) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REINFORCE_CITY;
		this.name = "Reinforce City";
		this.general = (General)startedBy.assignedRole;
	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(citizen.id == this.general.citizen.id){
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
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
}
