using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampaignManager {
	public Citizen leader;
	public int campaignLimit;
	public List<Campaign> activeCampaigns;
	public List<Citizen> controlledGovernorsAndKings;
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
	}
	internal void UpdateControlledGovernorsAndKings(){
		this.controlledGovernorsAndKings.Clear ();
		for(int i = 0; i < this.leader.city.kingdom.cities.Count; i++){
			if(this.leader.city.kingdom.cities[i].governor.supportedCitizen.Contains(this.leader)){
				this.controlledGovernorsAndKings.Add (this.leader.city.kingdom.cities [i].governor);
			}
		}
	}
	internal void CreateCampaign(){
		if(this.activeCampaigns.Count < this.campaignLimit){
			CAMPAIGN campaignType = GetTypeOfCampaign ();
			if(campaignType != CAMPAIGN.NONE){
				if(campaignType == CAMPAIGN.OFFENSE){
					if (this.targetableAttackCities.Count > 0) {
						Campaign newCampaign = new Campaign (this.leader, this.targetableAttackCities [0], campaignType);
						newCampaign.rallyPoint = GetRallyPoint ();
						this.activeCampaigns.Add (newCampaign);
						this.MakeCityActive (campaignType, this.targetableAttackCities [0]);
					} else {
						campaignType = CAMPAIGN.DEFENSE;
						if(this.targetableDefenseCities.Count > 0){
							Campaign newCampaign = new Campaign (this.leader, this.targetableDefenseCities [0], campaignType);
							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (campaignType, this.targetableDefenseCities [0]);
						}else{
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
						}
					}
				}else{
					if (this.targetableDefenseCities.Count > 0) {
						Campaign newCampaign = new Campaign (this.leader, this.targetableDefenseCities [0], campaignType);
						this.activeCampaigns.Add (newCampaign);
						this.MakeCityActive (campaignType, this.targetableDefenseCities [0]);
					} else {
						campaignType = CAMPAIGN.OFFENSE;
						if(this.targetableAttackCities.Count > 0){
							Campaign newCampaign = new Campaign (this.leader, this.targetableAttackCities [0], campaignType);
							newCampaign.rallyPoint = GetRallyPoint ();
							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (campaignType, this.targetableAttackCities [0]);
						}else{
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
						}
					}
				}
			}
		}
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
	internal void MakeCityActive(CAMPAIGN campaignType, City city){
		if(campaignType == CAMPAIGN.OFFENSE){
			City targetCity = this.targetableAttackCities.Find (x => x.id == city.id);
			this.targetableAttackCities.Remove (targetCity);
			this.activeAttackCities.Add (targetCity);
		}else{
			City targetCity = this.targetableDefenseCities.Find (x => x.id == city.id);
			this.targetableDefenseCities.Remove (targetCity);
			this.activeDefenseCities.Add (targetCity);
		}
	}
	internal void MakeCityInactive(CAMPAIGN campaignType, City city){
		if(campaignType == CAMPAIGN.OFFENSE){
			City targetCity = this.activeAttackCities.Find (x => x.id == city.id);
			this.activeAttackCities.Remove (targetCity);
			if(!targetCity.isDead){
				this.targetableAttackCities.Add (targetCity);
			}
		}else{
			City targetCity = this.activeDefenseCities.Find (x => x.id == city.id);
			this.activeDefenseCities.Remove (targetCity);
			if(!targetCity.isDead){
				this.targetableDefenseCities.Add (targetCity);
			}
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
		Campaign chosenCampaign = GetCampaignByID (((General)general.assignedRole).campaignID);
		if(chosenCampaign.campaignType == CAMPAIGN.OFFENSE){
			if(((General)general.assignedRole).location == chosenCampaign.rallyPoint){
				((General)general.assignedRole).targetLocation = null;
				if(AreAllGeneralsOnRallyPoint(chosenCampaign)){
					AttackCityNow (chosenCampaign);
				}
			}else if(((General)general.assignedRole).location == chosenCampaign.targetCity.hexTile){

			}
		}else{
			if(((General)general.assignedRole).location == chosenCampaign.targetCity.hexTile){

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
	internal Campaign GetCampaignByID(int id){
		for(int i = 0; i < this.activeCampaigns.Count; i++){
			if(this.activeCampaigns[i].id == id){
				return this.activeCampaigns [i];
			}
		}
		return null;
	}
}
