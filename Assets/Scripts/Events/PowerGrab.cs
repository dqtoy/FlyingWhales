using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PowerGrab : GameEvent {

	public Citizen kingToOverthrow;

	public PowerGrab(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen kingToOverthrow) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.POWER_GRAB;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " wants to grab power.";
		this.durationInWeeks = 48;
		this.remainingWeeks = this.durationInWeeks;
		this.kingToOverthrow = this.startedBy.city.kingdom.king;

		this.startedBy.history.Add (new History (startMonth, startWeek, startYear, this.startedBy.name + " started gathering influence for his/her claim as next in line to the " + this.kingToOverthrow.city.kingdom.name + " throne.", HISTORY_IDENTIFIER.NONE));
		this.kingToOverthrow.city.hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		this.remainingWeeks -= 1;
		if (this.startedBy.isDead || this.kingToOverthrow.isDead || this.remainingWeeks <= 0) {
			this.DoneEvent();
			return;
		}
		int exhortationChance = Random.Range(0, 100);
		if (exhortationChance < 20) {
			//make exhortation event
			Citizen citizenToExhort = null;
			List<Citizen> citizensSupportingMe = this.startedBy.GetCitizensSupportingThisCitizen();
			if (citizensSupportingMe.Where (x => x.role == ROLE.GOVERNOR).ToList().Count <= 0) {
				List<HexTile> tilesInKingdom = this.startedByKingdom.GetAllHexTilesInKingdom();
				if (tilesInKingdom.Contains (this.startedBy.currentLocation)) {
					citizenToExhort = this.startedBy.currentLocation.city.governor;
				} else {
					List<Citizen> allGovernors = GameManager.Instance.GetAllCitizensOfType (ROLE.GOVERNOR);
					if (allGovernors.Count > 0) {
						citizenToExhort = allGovernors[Random.Range(0, allGovernors.Count)];
					}
				}
			} else {
				int citizenToExhortChance = Random.Range (0, 100);
				if (citizenToExhortChance < 80) {
					List<Citizen> governorsSupportingMe = citizensSupportingMe.Where(x => x.role == ROLE.GOVERNOR).ToList();
					for (int i = 0; i < governorsSupportingMe.Count; i++) {
						for (int j = 0; j < governorsSupportingMe[i].city.hexTile.connectedTiles.Count; j++) {
							if (governorsSupportingMe[i].city.hexTile.connectedTiles[j].isOccupied) {
								if (!citizensSupportingMe.Contains(governorsSupportingMe[i].city.hexTile.connectedTiles[j].city.governor)) {
									citizenToExhort = governorsSupportingMe[i].city.hexTile.connectedTiles[j].city.governor;
									break;
								}
							}
						}
						if (citizenToExhort != null) {
							break;
						}
					}
				} else {
					List<Kingdom> adjacentKingdoms = this.startedByKingdom.GetAdjacentKingdoms();
					for (int i = 0; i < adjacentKingdoms.Count; i++) {
						if (!citizensSupportingMe.Contains (adjacentKingdoms [i].king)) {
							citizenToExhort = adjacentKingdoms[i].king;
							break;
						}
					}
				}
			}

			Exhortation newExhortation = new Exhortation (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, 
				this.startedBy, this.startedBy, citizenToExhort);
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
	}

}
