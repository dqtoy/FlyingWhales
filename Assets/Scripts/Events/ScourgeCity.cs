using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScourgeCity : GameEvent {
	public Scourge scourge;
	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;
	public City targetCity;

	public ScourgeCity(int startWeek, int startMonth, int startYear, Citizen startedBy, Scourge scourge, City targetCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SCOURGE_CITY;
		this.name = "Scourge City";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = startedBy.city.kingdom;
		this.targetKingdom = targetCity.kingdom;
		this.targetCity = targetCity;
		this.scourge = scourge;
//		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "event_title");
//		newLogTitle.AddToFillers (this.raidedCity, this.raidedCity.name);
//
//		Log raidStartLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "start");
//		raidStartLog.AddToFillers (this.startedByCity, this.startedByCity.name);
//		raidStartLog.AddToFillers (this.raidedCity, this.raidedCity.name);

	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
        base.DoneCitizenAction(citizen);
		this.DestroyASettlementInTargetCity ();
		RelationshipKings relationship = this.targetKingdom.king.GetRelationshipWithCitizen (this.sourceKingdom.king);
		relationship.AdjustLikeness (-5, this);
		this.DoneEvent ();
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		this.startedBy.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent ();
		//		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void DestroyASettlementInTargetCity(){
		List<HexTile> structures = this.targetCity.structures;
		HexTile targetSettlement = structures [structures.Count - 1];
		if(targetSettlement != null){
			/*
             * Reset tile for now.
             * TODO: When ruined settlement sprites are provided, use those instead.
             * */
			this.targetCity.RemoveTileFromCity(targetSettlement);
		}
	}
}
