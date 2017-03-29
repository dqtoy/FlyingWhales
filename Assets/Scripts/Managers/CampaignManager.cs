﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CampaignManager {
	public Citizen leader;
	public int campaignLimit;
	public List<Campaign> activeCampaigns;
	public List<Citizen> controlledGovernorsAndKings;

	public List<CityWar> intlWarCities;
	public List<CityWar> civilWarCities;
	public List<CityWar> successionWarCities;
	public List<CityWar> defenseWarCities;

	public List<City> targetableAttackCities;
	public List<City> targetableDefenseCities;
	public List<City> activeAttackCities;
	public List<City> activeDefenseCities;

	public CampaignManager(Citizen leader){
		this.leader = leader;
		this.campaignLimit = leader.GetCampaignLimit ();
		this.activeCampaigns = new List<Campaign>();
		this.targetableAttackCities = new List<City> ();
		this.targetableDefenseCities = new List<City> ();
		this.activeAttackCities = new List<City> ();
		this.activeDefenseCities = new List<City> ();
		this.intlWarCities = new List<CityWar>();
		this.civilWarCities = new List<CityWar>();
		this.successionWarCities = new List<CityWar>();
	}
	internal void UpdateControlledGovernorsAndKings(){
		this.controlledGovernorsAndKings.Clear ();
		for(int i = 0; i < this.leader.city.kingdom.cities.Count; i++){
			if(this.leader.city.kingdom.cities[i].governor.supportedCitizen.Contains(this.leader)){
				this.controlledGovernorsAndKings.Add (this.leader.city.kingdom.cities [i].governor);
			}
		}
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.leader.city.kingdom.id){
				if(KingdomManager.Instance.allKingdoms[i].king.supportedCitizen.Contains(this.leader)){
					this.controlledGovernorsAndKings.Add (KingdomManager.Instance.allKingdoms[i].king); 
				}
			}
		}
	}
	internal void CreateCampaign(){
		if(this.activeCampaigns.Count < this.campaignLimit){
			int randomWarType = UnityEngine.Random.Range (0, 2);
			CAMPAIGN campaignType = GetTypeOfCampaign ();
			Campaign newCampaign = null;
			if(campaignType != CAMPAIGN.NONE){
				if(campaignType == CAMPAIGN.OFFENSE){
					
					List<City> civilWarCities = this.civilWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
					List<City> successionWarCities = this.successionWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
					List<City> intlWarCities = this.intlWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();

					if(civilWarCities.Count <= 0 && successionWarCities.Count <= 0 && intlWarCities.Count <= 0){
						campaignType = CAMPAIGN.DEFENSE;
						List<City> defenseCities = this.defenseWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
						if(defenseCities.Count > 0){
							defenseCities = defenseCities.OrderBy(x => x.incomingGenerals.Min(y => (int?)((General)y.assignedRole).daysBeforeArrival) ?? (GridMap.Instance.height*GridMap.Instance.width)).ToList();
							int neededArmy = this.targetableDefenseCities [0].GetTotalAttackerStrength ();
							if(this.targetableDefenseCities[0].incomingGenerals.Count <= 0){
								neededArmy = (int)(this.targetableDefenseCities.Sum (x => x.GetCityArmyStrength ()) * 0.25f);
							}
							newCampaign = new Campaign (this.leader, this.targetableDefenseCities [0], campaignType, WAR_TYPE.NONE, neededArmy);
							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (newCampaign);
						}else{
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
							return;
						}
//						
					} else {
						for (int i = 0; i < this.leader.city.kingdom.relationshipsWithOtherKingdoms.Count; i++) {
//							if(this.leader.city.kingdom.relationshipsWithOtherKingdoms[i].isAtWar && this.leader.city.kingdom.relationshipsWithOtherKingdoms[i].isAdjacent){
//								intlWarCities.AddRange (this.leader.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.cities.Where(x => !this.IsCityActive(x, campaignType)).ToList());
//							}
							WAR_TYPE warType = WAR_TYPE.NONE;
							City target = null;
							GetWarAndTarget (intlWarCities, civilWarCities, successionWarCities, ref warType, ref target);

							int neededArmy = (int)(this.targetableDefenseCities.Sum (x => x.GetCityArmyStrength ()) * 0.25f);
							newCampaign = new Campaign (this.leader, target, campaignType, warType, neededArmy);
							newCampaign.rallyPoint = GetRallyPoint ();
							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (newCampaign);
						}
					}
				}else{
					if (this.targetableDefenseCities.Count > 0) {
						this.targetableDefenseCities = this.targetableDefenseCities.OrderBy(x => x.incomingGenerals.Min(y => (int?)((General)y.assignedRole).daysBeforeArrival) ?? (GridMap.Instance.height*GridMap.Instance.width)).ToList();
						int neededArmy = this.targetableDefenseCities [0].GetTotalAttackerStrength ();
						if(this.targetableDefenseCities[0].incomingGenerals.Count <= 0){
							neededArmy = (int)(this.targetableDefenseCities.Sum (x => x.GetCityArmyStrength ()) * 0.25f);
						}
						newCampaign = new Campaign (this.leader, this.targetableDefenseCities [0], campaignType, WAR_TYPE.NONE, neededArmy);
						this.activeCampaigns.Add (newCampaign);
						this.MakeCityActive (campaignType, this.targetableDefenseCities [0]);
					} else {
						campaignType = CAMPAIGN.OFFENSE;
						if(this.targetableAttackCities.Count > 0){
							List<City> civilWarCities = this.leader.civilWars.Select (x => x.city).ToList ();
							List<City> successionWarCities = this.leader.successionWars.Select (x => x.city).ToList ();
							List<City> intlWarCities = new List<City> ();
							for(int i = 0; i < this.leader.city.kingdom.relationshipsWithOtherKingdoms.Count; i++){
								if(this.leader.city.kingdom.relationshipsWithOtherKingdoms[i].isAtWar && this.leader.city.kingdom.relationshipsWithOtherKingdoms[i].isAdjacent){
									intlWarCities.AddRange (this.leader.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.cities);
								}
							}
							WAR_TYPE warType = WAR_TYPE.NONE;
							City target = null;
							GetWarAndTarget (intlWarCities, civilWarCities, successionWarCities, ref warType, ref target);
							int neededArmy = (int)(this.targetableDefenseCities.Sum (x => x.GetCityArmyStrength ()) * 0.25f);
							newCampaign = new Campaign (this.leader, target, campaignType, warType, neededArmy);
							newCampaign.rallyPoint = GetRallyPoint ();
							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (campaignType, target);
						}else{
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
							return;
						}
					}
				}
				EventManager.Instance.onRegisterOnCampaign.Invoke (newCampaign);

			}
		}
	}
	internal bool IsCityActive(City city, CAMPAIGN campaignType){
		if(campaignType == CAMPAIGN.OFFENSE){
			for(int i = 0; i < this.activeAttackCities.Count; i++){
				if(city.id == this.activeAttackCities[i].id){
					return true;
				}
			}
			return false;
		}else{
			for(int i = 0; i < this.activeDefenseCities.Count; i++){
				if(city.id == this.activeDefenseCities[i].id){
					return true;
				}
			}
			return false;
		}
	}
	internal void UpdateDefenseCities(){
		this.targetableDefenseCities.Clear ();
		if(this.leader.isKing){
			for(int i = 0; i < this.leader.city.kingdom.cities.Count; i++){
				if(this.leader.city.kingdom.cities[i].governor.supportedRebellion == null){
					this.targetableDefenseCities.Add (this.leader.city.kingdom.cities [i]);
				}
			}
		}else{
			this.targetableDefenseCities.Add (this.leader.city);
		}
	}
	internal void GetWarAndTarget(List<City> intlWarCities, List<City> civilWarCities, List<City> successionWarCities, ref WAR_TYPE warType, ref City target){
		if(intlWarCities.Count > 0 && civilWarCities.Count > 0 && successionWarCities.Count > 0){
			int chance = UnityEngine.Random.Range (0, 3);
			if(chance == 0){
				warType = WAR_TYPE.INTERNATIONAL;
				target = intlWarCities [UnityEngine.Random.Range (0, intlWarCities.Count)];
			}else if(chance == 1){
				warType = WAR_TYPE.CIVIL;
				target = civilWarCities [UnityEngine.Random.Range (0, civilWarCities.Count)];
			}else{
				warType = WAR_TYPE.SUCCESSION;
				target = successionWarCities [UnityEngine.Random.Range (0, successionWarCities.Count)];
			}
		}else if(intlWarCities.Count > 0 && civilWarCities.Count > 0 && successionWarCities.Count <= 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				warType = WAR_TYPE.INTERNATIONAL;
				target = intlWarCities [UnityEngine.Random.Range (0, intlWarCities.Count)];
			}else if(chance == 1){
				warType = WAR_TYPE.CIVIL;
				target = civilWarCities [UnityEngine.Random.Range (0, civilWarCities.Count)];
			}
		}else if(intlWarCities.Count > 0 && civilWarCities.Count <= 0 && successionWarCities.Count > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				warType = WAR_TYPE.INTERNATIONAL;
				target = intlWarCities [UnityEngine.Random.Range (0, intlWarCities.Count)];
			}else if(chance == 1){
				warType = WAR_TYPE.SUCCESSION;
				target = successionWarCities [UnityEngine.Random.Range (0, successionWarCities.Count)];
			}
		}else if(intlWarCities.Count <= 0 && civilWarCities.Count > 0 && successionWarCities.Count > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				warType = WAR_TYPE.CIVIL;
				target = civilWarCities [UnityEngine.Random.Range (0, civilWarCities.Count)];
			}else if(chance == 1){
				warType = WAR_TYPE.SUCCESSION;
				target = successionWarCities [UnityEngine.Random.Range (0, successionWarCities.Count)];
			}
		}else if(intlWarCities.Count > 0 && civilWarCities.Count <= 0 && successionWarCities.Count <= 0){
			warType = WAR_TYPE.INTERNATIONAL;
			target = intlWarCities [UnityEngine.Random.Range (0, intlWarCities.Count)];
		}else if(intlWarCities.Count <= 0 && civilWarCities.Count > 0 && successionWarCities.Count <= 0){
			warType = WAR_TYPE.CIVIL;
			target = civilWarCities [UnityEngine.Random.Range (0, civilWarCities.Count)];
		}else if(intlWarCities.Count <= 0 && civilWarCities.Count <= 0 && successionWarCities.Count > 0){
			warType = WAR_TYPE.SUCCESSION;
			target = successionWarCities [UnityEngine.Random.Range (0, successionWarCities.Count)];
		}else{
			warType = WAR_TYPE.NONE;
			target = null;
		}
	}
	internal void CampaignDone(Campaign campaign){
		Campaign doneCampaign = SearchCampaignByID (campaign.id);
		for(int i = 0; i < doneCampaign.registeredGenerals.Count; i++){
			UnregisterGenerals (doneCampaign.registeredGenerals [i], doneCampaign);
		}
		this.MakeCityInactive (doneCampaign);
		this.activeCampaigns.Remove (doneCampaign);
		doneCampaign = null;
		CreateCampaign ();
	}
	internal void UnregisterGenerals(Citizen general, Campaign chosenCampaign){
		((General)general.assignedRole).targetLocation = null;
		((General)general.assignedRole).warLeader = null;
		((General)general.assignedRole).campaignID = 0;
		((General)general.assignedRole).assignedCampaign = CAMPAIGN.NONE;
		((General)general.assignedRole).targetCity = null;
		((General)general.assignedRole).daysBeforeArrival = 0;

		chosenCampaign.registeredGenerals.Remove (general);
	}
	internal HexTile GetRallyPoint(){
		return null;
	}
	internal CAMPAIGN GetTypeOfCampaign(){
		int noOfOffenseCampaigns = 0;
		int noOfDefenseCampaigns = 0;
		for(int i = 0; i < this.activeCampaigns.Count; i++){
			if(this.activeCampaigns[i] != null){
				if(this.activeCampaigns[i].campaignType == CAMPAIGN.OFFENSE){
					noOfOffenseCampaigns += 1;
				}else if(this.activeCampaigns[i].campaignType == CAMPAIGN.DEFENSE){
					noOfDefenseCampaigns += 1;
				}
			}
		}
		if(this.leader.behaviorTraits.Contains(BEHAVIOR_TRAIT.AGGRESSIVE)){
			if(noOfOffenseCampaigns >= 2){
				return CAMPAIGN.DEFENSE;
			}else{
				return CAMPAIGN.OFFENSE;
			}
		}else if(this.leader.behaviorTraits.Contains(BEHAVIOR_TRAIT.DEFENSIVE)){
			if(noOfDefenseCampaigns >= 2){
				return CAMPAIGN.OFFENSE;
			}else{
				return CAMPAIGN.DEFENSE;
			}
		}else{
			if(noOfDefenseCampaigns > 0 && noOfOffenseCampaigns <= 0){
				return CAMPAIGN.OFFENSE;
			}else if(noOfDefenseCampaigns <= 0 && noOfOffenseCampaigns > 0){
				return CAMPAIGN.DEFENSE;
			}else if(noOfDefenseCampaigns > 0 && noOfOffenseCampaigns > 0){
				return CAMPAIGN.DEFENSE;
			}else{
				int chance = UnityEngine.Random.Range (0, 100);
				if(chance < 75){
					return CAMPAIGN.DEFENSE;
				}else{
					return CAMPAIGN.OFFENSE;
				}
			}
		}
	}
	internal void MakeCityActive(Campaign campaign){
		CityWar cityWar = null;
		if(campaign.warType == WAR_TYPE.INTERNATIONAL){
			cityWar = this.intlWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}else if(campaign.warType == WAR_TYPE.CIVIL){
			cityWar = this.civilWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}else if(campaign.warType == WAR_TYPE.SUCCESSION){
			cityWar = this.successionWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}else if(campaign.warType == WAR_TYPE.NONE){
			cityWar = this.defenseWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}
		if(cityWar != null){
			cityWar.isActive = true;
		}
//		if(campaign.campaignType == CAMPAIGN.OFFENSE){
//			CityWar cityWar = null;
//			if(campaign.warType == WAR_TYPE.INTERNATIONAL){
//				cityWar = this.intlWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			}else if(campaign.warType == WAR_TYPE.CIVIL){
//				cityWar = this.civilWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			}else if(campaign.warType == WAR_TYPE.SUCCESSION){
//				cityWar = this.successionWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			}
//			if(cityWar != null){
//				cityWar.isActive = true;
//			}
//		}else{
//			CityWar cityWar = this.defenseWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			if(cityWar != null){
//				cityWar.isActive = true;
//			}
////			CityWar cityWar = null;
////			if(campaign.warType == WAR_TYPE.INTERNATIONAL){
////				cityWar = this.intlWarCities.Find (x => x.city.id == campaign.targetCity.id);
////			}else if(campaign.warType == WAR_TYPE.CIVIL){
////				cityWar = this.civilWarCities.Find (x => x.city.id == campaign.targetCity.id);
////			}else if(campaign.warType == WAR_TYPE.SUCCESSION){
////				cityWar = this.successionWarCities.Find (x => x.city.id == campaign.targetCity.id);
////			}
////			if(cityWar != null){
////				cityWar.isActive = true;
////			}
//		}
	}
	internal void MakeCityInactive(Campaign campaign){
		CityWar cityWar = null;
		if(campaign.warType == WAR_TYPE.INTERNATIONAL){
			cityWar = this.intlWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}else if(campaign.warType == WAR_TYPE.CIVIL){
			cityWar = this.civilWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}else if(campaign.warType == WAR_TYPE.SUCCESSION){
			cityWar = this.successionWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}else if(campaign.warType == WAR_TYPE.NONE){
			cityWar = this.defenseWarCities.Find (x => x.city.id == campaign.targetCity.id);
		}
		if(cityWar != null){
			cityWar.isActive = false;
			if(cityWar.city.isDead){
				CitysDeath (cityWar);
			}
		}
//		if(campaign.campaignType == CAMPAIGN.OFFENSE){
//			
//		}else{
//			CityWar cityWar = this.defenseWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			if(cityWar != null){
//				cityWar.isActive = false;
//			}
//			CityWar cityWar = null;
//			if(campaign.warType == WAR_TYPE.INTERNATIONAL){
//				cityWar = this.intlWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			}else if(campaign.warType == WAR_TYPE.CIVIL){
//				cityWar = this.civilWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			}else if(campaign.warType == WAR_TYPE.SUCCESSION){
//				cityWar = this.successionWarCities.Find (x => x.city.id == campaign.targetCity.id);
//			}
//			if(cityWar != null){
//				cityWar.isActive = true;
//			}
//		}
	}
	internal void CitysDeath(CityWar cityWar){
		if(cityWar.warType == WAR_TYPE.INTERNATIONAL){
			this.intlWarCities.Remove (cityWar);
		}else if(cityWar.warType == WAR_TYPE.CIVIL){
			this.civilWarCities.Remove (cityWar);
		}else if(cityWar.warType == WAR_TYPE.SUCCESSION){
			this.successionWarCities.Remove (cityWar);
		}else if(cityWar.warType == WAR_TYPE.NONE){
			this.defenseWarCities.Remove (cityWar);
		}
	}
	internal int GetGeneralCountByPercentage(float percent){
		int totalGenerals = 0;
		for(int i = 0; i < this.controlledGovernorsAndKings.Count; i++){
			if(this.controlledGovernorsAndKings[i].isGovernor){
				for(int j = 0; j < this.controlledGovernorsAndKings[i].city.citizens.Count; j++){
					if(this.controlledGovernorsAndKings[i].city.citizens[j].role == ROLE.GENERAL){
						totalGenerals += 1;
					}
				}
			}
			if (this.controlledGovernorsAndKings [i].isKing) {
				for(int j = 0; j < this.controlledGovernorsAndKings[i].city.kingdom.cities.Count; j++){
					for(int k = 0; k < this.controlledGovernorsAndKings[i].city.kingdom.cities[j].citizens.Count; k++){
						if (this.controlledGovernorsAndKings [i].city.kingdom.cities [j].citizens [k].role == ROLE.GENERAL) {
							totalGenerals += 1;
						}
					}
				}
			}
		}
		float percentage = (percent / 100f) * totalGenerals;

		return (int)percentage;
	}

	internal void GeneralHasArrived(Citizen general){
		((General)general.assignedRole).targetLocation = null;
		Campaign chosenCampaign = SearchCampaignByID (((General)general.assignedRole).campaignID);
		if(chosenCampaign.campaignType == CAMPAIGN.OFFENSE){
			if(((General)general.assignedRole).location == chosenCampaign.rallyPoint){
				if(AreAllGeneralsOnRallyPoint(chosenCampaign)){
					AttackCityNow (chosenCampaign);
				}
			}else if(((General)general.assignedRole).location == chosenCampaign.targetCity.hexTile){
				//InitiateBattle
			}
		}else{
			if(((General)general.assignedRole).location == chosenCampaign.targetCity.hexTile){
				//InitiateDefense
			}
		}

	}
	internal void AttackCityNow(Campaign chosenCampaign){
		for(int i = 0; i < chosenCampaign.registeredGenerals.Count; i++){
			((General)chosenCampaign.registeredGenerals[i].assignedRole).targetLocation = chosenCampaign.targetCity.hexTile;
		}
	}
	internal bool AreAllGeneralsOnRallyPoint(Campaign chosenCampaign){
		for(int i = 0; i < chosenCampaign.registeredGenerals.Count; i++){
			if(((General)chosenCampaign.registeredGenerals[i].assignedRole).location != chosenCampaign.rallyPoint){
				return false;
			}
		}
		return true;
	}
	internal Campaign SearchCampaignByID(int id){
		for(int i = 0; i < this.activeCampaigns.Count; i++){
			if(this.activeCampaigns[i].id == id){
				return this.activeCampaigns [i];
			}
		}
		return null;
	}
}
