using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expansion : GameEvent {
	protected bool isExpanding = false;
	protected City originCity;

	internal HexTile hexTileToExpandTo = null;
	internal List<HexTile> path = null;
	internal GameObject expansionAvatar = null;

	public Expansion(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile targetHextile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXPANSION;
		this.description = startedBy.city.kingdom.king.name + " is looking looking to expand his kingdom and has funded and expedition led by " + startedBy.name;

		this.originCity = startedBy.city;
		this.hexTileToExpandTo = targetHextile;

		Debug.LogError(this.description);

		EventManager.Instance.AddEventToDictionary(this);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "event_title");
		newLogTitle.AddToFillers (null, startedBy.city.kingdom.name);


		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "start");
		newLog.AddToFillers (startedBy, startedBy.name);
		newLog.AddToFillers (startedBy.city, startedBy.city.name);

		InitializeExpansion ();

		this.EventIsCreated ();

	}

	internal void InitializeExpansion(){
		this.path = PathGenerator.Instance.GetPath (this.startedBy.city.hexTile, this.hexTileToExpandTo, PATHFINDING_MODE.COMBAT).ToList();
		this.expansionAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/ExpansionAvatar"), this.startedBy.city.hexTile.transform) as GameObject;
		this.expansionAvatar.transform.localPosition = Vector3.zero;
		this.expansionAvatar.GetComponent<ExpansionAvatar>().Init(this);
	}
	internal void ExpandToTargetHextile(){
		if(this.hexTileToExpandTo.city == null || this.hexTileToExpandTo.city.id == 0){
			this.startedByKingdom.AddTileToKingdom(this.hexTileToExpandTo);
			this.hexTileToExpandTo.city.ExpandToThisCity(this.startedBy);

			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "expand");
			newLog.AddToFillers (this.hexTileToExpandTo.city, this.hexTileToExpandTo.city.name);

		}else{
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
	internal void DeathByOtherReasons(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_other");
		newLog.AddToFillers (this.startedBy, this.startedBy.name);
		newLog.AddToFillers (null, this.startedBy.deathReasonText);

		this.DoneEvent ();
	}
	internal void DeathByGeneral(General general){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_general");
		newLog.AddToFillers (general.citizen, general.citizen.name);

		this.startedBy.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent ();
	}

	internal override void DoneEvent(){
		GameObject.Destroy (this.expansionAvatar);
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
}
