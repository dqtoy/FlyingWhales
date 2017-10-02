using UnityEngine;
using System.Collections;

public class AttackCity : GameEvent {

	internal General general;
	internal City sourceCity;
	internal City targetCity;
	internal GameEvent gameEvent;

	public AttackCity(int startWeek, int startMonth, int startYear, Citizen startedBy, General general, City sourceCity, City targetCity, GameEvent gameEvent) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ATTACK_CITY;
		this.name = "Attack City";
		//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.general = general;
		this.sourceCity = sourceCity;
		this.targetCity = targetCity;
		this.gameEvent = gameEvent;
		if(this.gameEvent is Rebellions){
			int power = ((Rebellions)this.gameEvent).dividedPower;
			if(power > this.sourceCity.weapons){
				power = this.sourceCity.weapons;
			}
			this.sourceCity.AdjustWeapons (-power);
			this.general.damage = power;
		}
		Debug.Log (general.citizen.name + " of " + general.citizen.city.kingdom.name + " will attack " + targetCity.name);
//		Messenger.AddListener("OnDayEnd", this.PerformAction);

		//		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
		//		newLogTitle.AddToFillers (visitor.citizen, visitor.citizen.name);
		//		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
		//		newLog.AddToFillers (visitor.citizen, visitor.citizen.name);
		//		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		//		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);

		//		EventManager.Instance.AddEventToDictionary (this);
		//		this.EventIsCreated ();

	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen) {
        base.DoneCitizenAction(citizen);
		if(this.general != null){
			if(citizen.id == this.general.citizen.id){
				CombatManager.Instance.CityBattle (this.targetCity, this.general);
				this.general.citizen.Death (DEATH_REASONS.BATTLE);
				if(this.gameEvent is Wars){
					if(!this.targetCity.isDead){
						((Wars)this.gameEvent).ChangeTurn ();
					}
				}else if(this.gameEvent is Rebellions){
					((Rebellions)this.gameEvent).CheckTurns ();
				}
			}
		}
		this.DoneEvent ();
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
		this.general.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent ();
	}
	#endregion
}
