using UnityEngine;
using System.Collections;

public class Provocation : GameEvent {
	internal Kingdom sourceKingdom;
	internal Kingdom targetKingdom;
	internal City targetCity;
	internal Provoker provoker;

	public Provocation(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom, Provoker provoker) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.PROVOCATION;
		this.name = "Provocation";
		this.durationInDays = EventManager.Instance.eventDuration [this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = sourceKingdom;
		this.targetKingdom = targetKingdom;
		this.provoker = provoker;
		this.targetCity = targetKingdom.capitalCity;

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Provocation", "event_title");
		newLogTitle.AddToFillers (null, this.targetCity.name, LOG_IDENTIFIER.CITY_1);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Provocation", "start");
		newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.provoker.citizen, this.provoker.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.targetCity, this.targetCity.name, LOG_IDENTIFIER.CITY_1);

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
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
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
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Provocation", "provoke");
			newLog.AddToFillers (this.provoker.citizen, this.provoker.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLog.AddToFillers (this.targetCity, this.targetCity.name, LOG_IDENTIFIER.CITY_1);
			newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		}
		this.DoneEvent ();
	}
}
