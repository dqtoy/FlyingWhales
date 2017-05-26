using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expansion : GameEvent {

	protected int recruitmentPeriodInWeeks;
	protected int remainingRecruitmentPeriodInWeeks;

//	protected List<Citizen> citizensJoiningExpansion = new List<Citizen>();

	protected bool isExpanding = false;
	protected City originCity;

	internal HexTile hexTileToExpandTo = null;
	internal List<HexTile> path = null;
	internal GameObject expansionAvatar = null;

	public Expansion(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile targetHextile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXPANSION;
		this.description = startedBy.city.kingdom.king.name + " is looking looking to expand his kingdom and has funded and expedition led by " + startedBy.name;
//		this.durationInDays = 4;
		this.remainingDays = this.durationInDays;

		this.recruitmentPeriodInWeeks = 4;
		this.remainingRecruitmentPeriodInWeeks = this.recruitmentPeriodInWeeks;
		this.originCity = startedBy.city;
		this.hexTileToExpandTo = targetHextile;

		Debug.LogError(this.description);

		this.startedBy.city.hexTile.AddEventOnTile(this);

		this.startedBy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			"Sent an expedition to look for new habitable lands for his kingdom " + startedBy.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

		this.startedByCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			"Sent an expedition to look for new habitable lands." , HISTORY_IDENTIFIER.NONE));
		
//		this.citizensJoiningExpansion.Add (this.startedBy);

//		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
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

			this.resolution = "Expansion was successful, new city " + this.hexTileToExpandTo.city.name + " was added to " + this.startedByKingdom.name + ".";
			this.startedByCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
				"Successful Expansion to " + this.hexTileToExpandTo.city.name, HISTORY_IDENTIFIER.NONE));
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
	/*internal override void PerformAction(){
		if (this.remainingDays > 0) {
			this.remainingDays -= 1;
		} 
		if(this.remainingDays <= 0) {
			int disappearChance = Random.Range(0,100);
			if (disappearChance < 15) {
				//Disappear
				this.resolution = "Expansion Citizens suddenly disappeared.";
				for (int i = 0; i < this.citizensJoiningExpansion.Count; i++) {
					this.citizensJoiningExpansion[i].Death(DEATH_REASONS.DISAPPEARED_EXPANSION);
				}
				this.DoneEvent();
			} else {
				//Expand
				if (this.hexTileToExpandTo == null) {
					this.hexTileToExpandTo = CityGenerator.Instance.GetNearestHabitableTile (originCity);
				}
				if (this.hexTileToExpandTo != null) {
					this.startedByKingdom.AddTileToKingdom(this.hexTileToExpandTo);
//					this.startedBy.city.RemoveCitizenFromCity(this.startedBy);
					this.hexTileToExpandTo.city.ExpandToThisCity(this.citizensJoiningExpansion);

					this.resolution = "Expansion was successful, new city " + this.hexTileToExpandTo.city.name + " was added to " + this.startedByKingdom.name + ".";
					this.startedByCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
						"Successful Expansion to " + this.hexTileToExpandTo.city.name, HISTORY_IDENTIFIER.NONE));
					this.DoneEvent ();
				} else {
					this.resolution = "Expansion Citizens we're killed.";
					this.DoneEvent ();
				}
			}
		}


//		if (this.remainingRecruitmentPeriodInWeeks > 0) {
//			this.Recruit ();
//		} else {
//			if (!this.isExpanding) {
//				if (this.citizensJoiningExpansion.Where (x => x.age >= 16).ToList ().Count < 12) {
//					this.isExpanding = true;
//				} else {
//					this.resolution = "Expansion was cancelled because not enough citizens were recruited";
//					this.DoneEvent ();
//					return;
//				}
//			}
//
//			if (this.isExpanding) {
//				if (this.remainingWeeks > 0) {
//					this.remainingWeeks -= 1;
//				} else {
//					
//					int disappearChance = Random.Range(0,100);
//					if (disappearChance < 15) {
//						//Disappear
//						this.resolution = "Expansion Citizens suddenly disappeared.";
//						this.DoneEvent();
//					} else {
//						//Expand
//						if (originCity.adjacentHabitableTiles.Count > 0) {
//							HexTile hexTileToExpandTo = originCity.adjacentHabitableTiles [Random.Range (0, originCity.adjacentHabitableTiles.Count)];
//							this.startedByKingdom.AddTileToKingdom(hexTileToExpandTo);
//							hexTileToExpandTo.city.ExpandToThisCity(this.citizensJoiningExpansion);
//							this.resolution = "Expansion was successful, new city " + hexTileToExpandTo.city.name + " was added to " + this.startedByKingdom.name + ".";
//							this.DoneEvent ();
//						} else {
//							this.resolution = "Expansion Citizens we're killed.";
//							this.DoneEvent ();
//						}
//					}
//				}
//			}
//		}
	}*/


//	internal void AddCitizensToExpansion(List<Citizen> citizensToAdd){
//		citizensJoiningExpansion.AddRange(citizensToAdd);
//	}

	protected void Recruit(){
		this.remainingRecruitmentPeriodInWeeks -= 1;
		EventManager.Instance.onRecruitCitizensForExpansion.Invoke (this, this.startedByKingdom);
	}

	internal override void DoneEvent(){
		GameObject.Destroy (this.expansionAvatar);
		Debug.LogError (this.resolution);
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
//		EventManager.Instance.onGameEventEnded.Invoke(this);
	}
}
