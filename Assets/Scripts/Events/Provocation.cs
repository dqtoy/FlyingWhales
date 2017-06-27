using UnityEngine;
using System.Collections;

public class Provocation : GameEvent {
	internal Kingdom sourceKingdom;
	internal Kingdom targetKingdom;
	internal Provoker provoker;

	public Provocation(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom, Provoker provoker) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.PROVOCATION;
		this.name = "Provocation";
		this.durationInDays = EventManager.Instance.eventDuration [this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = sourceKingdom;
		this.targetKingdom = targetKingdom;
		this.provoker = provoker;

		this.EventIsCreated ();
	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen){
		base.DoneCitizenAction (citizen);
		IncreaseUnrest ();
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
		this.provoker.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void IncreaseUnrest(){
		if(this.targetKingdom.isAlive()){
			this.targetKingdom.AdjustUnrest (10);
		}
	}
}
