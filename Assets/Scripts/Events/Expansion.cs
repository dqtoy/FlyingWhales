using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expansion : GameEvent {

	protected int recruitmentPeriodInWeeks;
	protected int remainingRecruitmentPeriodInWeeks;

	protected List<Citizen> citizensJoiningExpansion = new List<Citizen>();

	protected bool isExpanding = false;
	protected City originCity;

	public Expansion(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXPANSION;
		this.description = startedBy.city.kingdom.king.name + " is looking looking to expand his kingdom and has funded and expedition led by " + startedBy.name;
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;

		this.recruitmentPeriodInWeeks = 4;
		this.remainingRecruitmentPeriodInWeeks = this.recruitmentPeriodInWeeks;
		this.originCity = startedBy.city;

		Debug.LogError (this.description);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingRecruitmentPeriodInWeeks > 0) {
			this.Recruit ();
		} else {
			if (!this.isExpanding) {
				if (this.citizensJoiningExpansion.Where (x => x.age >= 16).ToList ().Count < 12) {
					this.isExpanding = true;
				} else {
					this.resolution = "Expansion was cancelled because not enough citizens were recruited";
					this.DoneEvent ();
					return;
				}
			}

			if (this.isExpanding) {
				if (this.remainingWeeks > 0) {
					this.remainingWeeks -= 1;
				} else {
					
					int disappearChance = Random.Range(0,100);
					if (disappearChance < 15) {
						//Disappear
						this.resolution = "Expansion Citizens suddenly disappeared.";
						this.DoneEvent();
					} else {
						//Expand
						if (originCity.adjacentHabitableTiles.Count > 0) {
							HexTile hexTileToExpandTo = originCity.adjacentHabitableTiles [Random.Range (0, originCity.adjacentHabitableTiles.Count)];
							this.startedByKingdom.AddTileToKingdom(hexTileToExpandTo);
							hexTileToExpandTo.city.ExpandToThisCity(this.citizensJoiningExpansion);
							this.resolution = "Expansion was successful, new city " + hexTileToExpandTo.city.name + " was added to " + this.startedByKingdom.name + ".";
							this.DoneEvent ();
						} else {
							this.resolution = "Expansion Citizens we're killed.";
							this.DoneEvent ();
						}
					}
				}
			}
		}
	}

	internal void AddCitizensToExpansion(List<Citizen> citizensToAdd){
		citizensJoiningExpansion.AddRange(citizensToAdd);
	}

	protected void Recruit(){
		this.remainingRecruitmentPeriodInWeeks -= 1;
		EventManager.Instance.onRecruitCitizensForExpansion.Invoke (this, this.startedByKingdom);
	}

	internal override void DoneEvent(){
		Debug.LogError (this.resolution);
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
}
