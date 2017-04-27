using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PowerGrab : GameEvent {

	public Citizen kingToOverthrow;
	public List<Citizen> uncovered;
	public List<Citizen> exhortedCitizens;

	public PowerGrab(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen kingToOverthrow) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.POWER_GRAB;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " wants to grab power.";
		this.durationInWeeks = 48;
		this.remainingWeeks = this.durationInWeeks;
		this.kingToOverthrow = this.startedBy.city.kingdom.king;
		this.uncovered = new List<Citizen>();
		this.exhortedCitizens = new List<Citizen>();

		this.startedBy.history.Add (new History (startMonth, startWeek, startYear, this.startedBy.name + " started gathering influence for his/her claim as next in line to the " + this.kingToOverthrow.city.kingdom.name + " throne.", HISTORY_IDENTIFIER.NONE));
		this.kingToOverthrow.city.hexTile.AddEventOnTile(this);
		Debug.LogError (this.description);
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
				if (this.startedBy.homeKingdom.id == this.startedBy.city.kingdom.id) {
					//if governor is at his home kingdom
					if (this.startedBy.city.governor.id == this.startedBy.id) {
						List<Citizen> allGovernorsInKingdom = this.startedBy.city.kingdom.GetAllCitizensOfType (ROLE.GOVERNOR);
						allGovernorsInKingdom.Remove(this.startedBy);
						if (allGovernorsInKingdom.Count > 0) {
							citizenToExhort = allGovernorsInKingdom [Random.Range (0, allGovernorsInKingdom.Count)];
						}
					} else {
						citizenToExhort = this.startedBy.city.governor;
					}
				} else {
					//if governor is at outside his home kingdom
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
			if(citizenToExhort != null){
				List<Citizen> envoys = this.startedByCity.GetCitizensWithRole(ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
				if (envoys.Count > 0) {
					Exhortation newExhortation = new Exhortation (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, 
						this.startedBy, envoys[0], citizenToExhort, this);
					((Envoy)envoys [0].assignedRole).inAction = true;
				} else {
					Exhortation newExhortation = new Exhortation (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, 
						this.startedBy, this.startedBy, citizenToExhort, this);
				}
			}else{
				Debug.Log ("CAN'T EXHORT! THERE IS NO CITIZEN TO EXHORT!");
			}
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		Debug.LogError (this.startedBy.name + " has ended power grab.");
	}

}
