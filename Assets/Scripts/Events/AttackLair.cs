using UnityEngine;
using System.Collections;

public class AttackLair : GameEvent {

	internal General general;
	internal HexTile targetHextile;

	public AttackLair(int startWeek, int startMonth, int startYear, Citizen startedBy, General general, HexTile targetHextile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ATTACK_LAIR;
		this.name = "Attack Lair";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.general = general;
		this.targetHextile = targetHextile;
	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen) {
        base.DoneCitizenAction(citizen);
		if(this.general != null){
			if(citizen.id == this.general.citizen.id){
				if(this.targetHextile.lair != null){
					CombatManager.Instance.LairBattle (this.targetHextile.lair, this.general);
					this.general.citizen.Death (DEATH_REASONS.BATTLE);
				}
			}
		}
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent ();
//		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.general.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent ();
	}
	#endregion
}
