using UnityEngine;
using System.Collections;

public class HuntLair : GameEvent {

	private Ranger _ranger;
	private Lair _targetLair;

	public Lair targetLair{
		get{return this._targetLair;}
	}

	public Ranger ranger{
		get{return this._ranger;}
	}

	public HuntLair(int startWeek, int startMonth, int startYear, Citizen startedBy, Ranger ranger, Lair targetLair) : base(startWeek, startMonth, startYear, startedBy) {
		this.eventType = EVENT_TYPES.HUNT_LAIR;
		this.name = "Hunt Lair";
		this.durationInDays = EventManager.Instance.eventDuration [this.eventType];
		this._ranger = ranger;
		this._targetLair = targetLair;

		EventManager.Instance.AddEventToDictionary(this);

	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen) {
		base.DoneCitizenAction(citizen);
		if(this._ranger != null){
			if(citizen.id == this._ranger.citizen.id){
				if(this._ranger.location.lair != null){
					CombatManager.Instance.LairBattle (this._ranger.location.lair, this._ranger);
				}
				this._ranger.citizen.Death (DEATH_REASONS.BATTLE);
			}
		}
		this.DoneEvent();
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
		//		Messenger.RemoveListener("OnDayEnd", this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this._ranger.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent ();
	}
	#endregion
}
