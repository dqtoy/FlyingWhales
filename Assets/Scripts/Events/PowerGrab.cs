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
		this.durationInDays = 48;
		this.remainingDays = this.durationInDays;
		this.kingToOverthrow = this.startedBy.city.kingdom.king;
		this.uncovered = new List<Citizen>();
		this.exhortedCitizens = new List<Citizen>();

		this.startedBy.UnsupportCitizen (this.startedBy.supportedCitizen);
		this.startedBy.supportedCitizen = this.startedBy;

		Debug.LogError (this.description);
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.resolution = this.startedBy.name + " exhorted a total of " + this.exhortedCitizens.Count.ToString() + " citizens.";
			this.DoneEvent();
			return;
		}
		if(this.kingToOverthrow.isDead){
			this.resolution = this.kingToOverthrow.name + " died before the event ended.";
			this.DoneEvent();
			return;
		}

		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event ended.";
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
						List<Citizen> allGovernorsInKingdom = this.startedBy.city.kingdom.GetAllCitizensOfType (ROLE.GOVERNOR).
							Where(x => 
								(x.supportedCitizen == null || (x.supportedCitizen != null && x.supportedCitizen.id != this.startedBy.id && x.supportedCitizen.id != x.id && !IsCitizenFirstInLine(x)))
								).ToList();
						allGovernorsInKingdom.Remove(this.startedBy);
						if (allGovernorsInKingdom.Count > 0) {
							citizenToExhort = allGovernorsInKingdom [Random.Range (0, allGovernorsInKingdom.Count)];
						}
					} else {
						if(this.startedBy.city.governor.supportedCitizen == null){
							citizenToExhort = this.startedBy.city.governor;
						}else{
							if (this.startedBy.city.governor.supportedCitizen.id != this.startedBy.city.governor.id &&
								!citizensSupportingMe.Contains(this.startedBy.city.governor)) {
								citizenToExhort = this.startedBy.city.governor;
							} else {
								List<Citizen> allGovernorsInKingdom = this.startedBy.city.kingdom.GetAllCitizensOfType (ROLE.GOVERNOR).
									Where(x => 
										(x.supportedCitizen == null || (x.supportedCitizen != null && x.supportedCitizen.id != this.startedBy.id && x.supportedCitizen.id != x.id)
											&& !IsCitizenFirstInLine(x))
									).ToList();
								allGovernorsInKingdom.Remove(this.startedBy);
								if (allGovernorsInKingdom.Count > 0) {
									citizenToExhort = allGovernorsInKingdom [Random.Range (0, allGovernorsInKingdom.Count)];
								}
							}
						}
					
					}
				} 
			} else {
				int citizenToExhortChance = Random.Range (0, 100);
				if (citizenToExhortChance < 80) {
					List<Citizen> governorsSupportingMe = citizensSupportingMe.Where(x => x.role == ROLE.GOVERNOR).ToList();
					for (int i = 0; i < governorsSupportingMe.Count; i++) {
						for (int j = 0; j < governorsSupportingMe[i].city.hexTile.connectedTiles.Count; j++) {
							HexTile adjacentTile = governorsSupportingMe [i].city.hexTile.connectedTiles [j];
							if (adjacentTile.isOccupied && adjacentTile.city.kingdom.id == this.startedBy.city.kingdom.id && !IsCitizenFirstInLine(adjacentTile.city.governor)) {
								if (!citizensSupportingMe.Contains(adjacentTile.city.governor)) {
									citizenToExhort = adjacentTile.city.governor;
									break;
								}
							}
						}
						if (citizenToExhort != null) {
							break;
						}
					}
				} else {
					List<Kingdom> adjacentKingdoms = this.startedBy.city.kingdom.GetAdjacentKingdoms();
					for (int i = 0; i < adjacentKingdoms.Count; i++) {
						if (!citizensSupportingMe.Contains (adjacentKingdoms [i].king)) {
							citizenToExhort = adjacentKingdoms[i].king;
							break;
						}
					}
				}
			}

//			if(citizenToExhort != null){
//				List<Citizen> envoys = this.startedByCity.GetCitizensWithRole(ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
//				if (envoys.Count > 0) {
//					Exhortation newExhortation = new Exhortation (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
//						this.startedBy, envoys[0], citizenToExhort, this);
//					((Envoy)envoys [0].assignedRole).inAction = true;
//				} else {
//					Exhortation newExhortation = new Exhortation (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
//						this.startedBy, this.startedBy, citizenToExhort, this);
//				}
//			}else{
////				Debug.Log ("CAN'T EXHORT! THERE IS NO CITIZEN TO EXHORT!");
//			}
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
	}

	internal bool IsCitizenFirstInLine(Citizen citizen){
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
			if (currKingdom.successionLine.Count > 0) {
				if (currKingdom.successionLine[0].id == citizen.id) {
					return true;
				}
			}
		}
		return false;
	}

}
