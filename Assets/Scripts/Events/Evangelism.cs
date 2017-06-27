using UnityEngine;
using System.Collections;

public class Evangelism : GameEvent {
	internal Kingdom sourceKingdom;
	internal Kingdom targetKingdom;
	internal City targetCity;
	internal Missionary missionary;

	public Evangelism(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom, Missionary missionary) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EVANGELISM;
		this.name = "Evangelism";
		this.durationInDays = EventManager.Instance.eventDuration [this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = sourceKingdom;
		this.targetKingdom = targetKingdom;
		this.missionary = missionary;
		this.targetCity = targetKingdom.capitalCity;

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Evangelism", "event_title");
		newLogTitle.AddToFillers (null, this.targetCity.name, LOG_IDENTIFIER.CITY_1);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Evangelism", "start");
		newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this.targetCity, this.targetCity.name, LOG_IDENTIFIER.CITY_1);

		this.EventIsCreated ();
	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen){
		base.DoneCitizenAction (citizen);
		DecreaseUnrestAndImproveRelationship ();
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
		this.missionary.citizen.Death (DEATH_REASONS.BATTLE);
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

	private void DecreaseUnrestAndImproveRelationship(){
		if(this.targetKingdom.isAlive() && !this.targetCity.isDead){
			this.targetKingdom.AdjustUnrest (-10);
			if(this.sourceKingdom.isAlive()){
				RelationshipKings relationship = this.targetKingdom.king.GetRelationshipWithCitizen (this.sourceKingdom.king);
				relationship.AdjustLikeness (10, this);
			}
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Evangelism", "evangelize");
			newLog.AddToFillers (this.targetCity, this.targetCity.name, LOG_IDENTIFIER.CITY_1);
			newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
			newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
	}
}
