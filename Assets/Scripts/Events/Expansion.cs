﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expansion : GameEvent {
	protected bool isExpanding = false;
	protected City originCity;

	internal HexTile hexTileToExpandTo = null;
	internal Expander expander;
	public Expansion(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile targetHextile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXPANSION;
		this.name = "Expansion";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.description = startedBy.city.kingdom.king.name + " is looking looking to expand his kingdom and has funded and expedition led by " + startedBy.name;

		this.originCity = startedBy.city;
		this.hexTileToExpandTo = targetHextile;
		this.hexTileToExpandTo.isTargeted = true;
		EventManager.Instance.AddEventToDictionary(this);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "event_title");
		newLogTitle.AddToFillers (null, startedBy.city.kingdom.name);


		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "start");
		newLog.AddToFillers (startedBy, startedBy.name);
		newLog.AddToFillers (startedBy.city, startedBy.city.name);

		this.EventIsCreated ();

	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
        base.DoneCitizenAction(citizen);
		if (this.hexTileToExpandTo.city == null || this.hexTileToExpandTo.city.id == 0) {
			this.startedByKingdom.CreateNewCityOnTileForKingdom (this.hexTileToExpandTo);
			this.hexTileToExpandTo.city.ExpandToThisCity (this.startedBy);

			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "expand");
			newLog.AddToFillers (this.hexTileToExpandTo.city, this.hexTileToExpandTo.city.name);

		} else {
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "beaten");
			this.startedBy.Death (DEATH_REASONS.DISAPPEARED_EXPANSION);
		}
		this.DoneEvent ();
	}
	internal void Disappearance(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "disappearance");
		this.startedBy.Death (DEATH_REASONS.DISAPPEARED_EXPANSION);
		this.DoneEvent();
	}
	internal override void DeathByOtherReasons(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_other");
		newLog.AddToFillers (this.startedBy, this.startedBy.name);
		newLog.AddToFillers (null, this.startedBy.deathReasonText);

		this.DoneEvent ();
	}
	internal override void DeathByGeneral(General general){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_general");
		newLog.AddToFillers (general.citizen, general.citizen.name);

		this.startedBy.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent ();
	}

	internal override void DoneEvent(){
		base.DoneEvent();
		this.hexTileToExpandTo.isTargeted = false;
//		this.expander.DestroyGO ();
//		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
}
