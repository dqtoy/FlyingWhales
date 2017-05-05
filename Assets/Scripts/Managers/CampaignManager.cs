using UnityEngine;
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
		this.defenseWarCities = new List<CityWar>();
	}
	internal void CreateCampaign(){
		if(this.activeCampaigns.Count < this.campaignLimit){
			int randomWarType = UnityEngine.Random.Range (0, 2);
			CAMPAIGN campaignType = GetTypeOfCampaign ();
			Campaign newCampaign = null;
			if(campaignType != CAMPAIGN.NONE){
				if(campaignType == CAMPAIGN.OFFENSE){
					
					List<City> civilWarCities = this.civilWarCities.Where(x => !x.isActive && x.city.HasAdjacency(this.leader.city.kingdom.id)).Select (x => x.city).ToList ();
					List<City> successionWarCities = this.successionWarCities.Where(x => !x.isActive && x.city.HasAdjacency(this.leader.city.kingdom.id)).Select (x => x.city).ToList ();
					List<City> intlWarCities = this.intlWarCities.Where(x => !x.isActive && x.city.HasAdjacency(this.leader.city.kingdom.id)).Select (x => x.city).ToList ();

					if(civilWarCities.Count <= 0 && successionWarCities.Count <= 0 && intlWarCities.Count <= 0){
						campaignType = CAMPAIGN.DEFENSE;
						List<City> defenseCities = this.defenseWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
						if(defenseCities.Count > 0){
							defenseCities = defenseCities.OrderBy(x => x.incomingGenerals.Where(y => y.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && y.targetCity.id == x.id).Min(y => (int?) y.daysBeforeArrival) ?? (GridMap.Instance.height*GridMap.Instance.width)).ToList();

							int nearestArrival = -2;
							int neededArmy = defenseCities [0].GetTotalAttackerStrength (ref nearestArrival);
							if(neededArmy <= 0){
								neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
							}
							newCampaign = new Campaign (this.leader, defenseCities[0], campaignType, WAR_TYPE.NONE, neededArmy, nearestArrival + 1);

							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (newCampaign);
						}else{
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
//							return;
						}
//						
					} else {
						WAR_TYPE warType = WAR_TYPE.NONE;
						City target = null;
						GetWarAndTarget (intlWarCities, civilWarCities, successionWarCities, ref warType, ref target);

						int neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
						newCampaign = new Campaign (this.leader, target, campaignType, warType, neededArmy);
						newCampaign.rallyPoint = GetRallyPoint (newCampaign);
						if(newCampaign.rallyPoint == null){
							Debug.Log (this.leader.name + " NO RALLY POINT for target " + target.name);
							newCampaign = null;
						}else{
							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (newCampaign);
						}

//						for (int i = 0; i < this.leader.city.kingdom.relationshipsWithOtherKingdoms.Count; i++) {
//							if(this.leader.city.kingdom.relationshipsWithOtherKingdoms[i].isAtWar && this.leader.city.kingdom.relationshipsWithOtherKingdoms[i].isAdjacent){
//								intlWarCities.AddRange (this.leader.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.cities.Where(x => !this.IsCityActive(x, campaignType)).ToList());
//							}

//						}
					}
				}else{
					List<City> defenseCities = this.defenseWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
					if(defenseCities.Count > 0){
						defenseCities = defenseCities.OrderBy(x => x.incomingGenerals.Where(y => y.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && y.targetCity.id == x.id).Min(y => (int?) y.daysBeforeArrival) ?? (GridMap.Instance.height*GridMap.Instance.width)).ToList();

						int nearestArrival = -2;
						int neededArmy = defenseCities [0].GetTotalAttackerStrength (ref nearestArrival);
						if(neededArmy <= 0){
							neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
						}
						newCampaign = new Campaign (this.leader, defenseCities[0], campaignType, WAR_TYPE.NONE, neededArmy, nearestArrival + 1);

						this.activeCampaigns.Add (newCampaign);
						this.MakeCityActive (newCampaign);
					}else{
						campaignType = CAMPAIGN.OFFENSE;

						List<City> civilWarCities = this.civilWarCities.Where(x => !x.isActive && x.city.HasAdjacency(this.leader.city.kingdom.id)).Select (x => x.city).ToList ();
						List<City> successionWarCities = this.successionWarCities.Where(x => !x.isActive && x.city.HasAdjacency(this.leader.city.kingdom.id)).Select (x => x.city).ToList ();
						List<City> intlWarCities = this.intlWarCities.Where(x => !x.isActive && x.city.HasAdjacency(this.leader.city.kingdom.id)).Select (x => x.city).ToList ();

						if(civilWarCities.Count <= 0 && successionWarCities.Count <= 0 && intlWarCities.Count <= 0){
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
//							return;
						}else{
							WAR_TYPE warType = WAR_TYPE.NONE;
							City target = null;
							GetWarAndTarget (intlWarCities, civilWarCities, successionWarCities, ref warType, ref target);

							int neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
							newCampaign = new Campaign (this.leader, target, campaignType, warType, neededArmy);
							newCampaign.rallyPoint = GetRallyPoint (newCampaign);
							if(newCampaign.rallyPoint == null){
								Debug.Log (this.leader.name + " NO RALLY POINT for target " + target.name);
								newCampaign = null;
							}else{
								this.activeCampaigns.Add (newCampaign);
								this.MakeCityActive (newCampaign);
							}
						}

					}
				}
				if(newCampaign != null){
					Debug.Log ("Created Campaign " + newCampaign.campaignType.ToString () + " " + newCampaign.targetCity.name);
					EventManager.Instance.onRegisterOnCampaign.Invoke (newCampaign);
				}else{
					//Create Ghost Campaign
					newCampaign = new Campaign (this.leader, null, CAMPAIGN.NONE, WAR_TYPE.NONE, 0);
					newCampaign.isGhost = true;
					Debug.Log ("Created Ghost Campaign " + newCampaign.campaignType.ToString ());

					this.activeCampaigns.Add (newCampaign);
				}
			}

			CreateCampaign ();
		}
	}
	private IEnumerator CheckCampaign(Campaign newCampaign){
		yield return null;
		if (newCampaign.registeredGenerals.Count <= 0) {
			//Destroy campaign without creating new
//			CampaignDone (newCampaign);
			CampaignDone(newCampaign, false);
			//Create Ghost Campaign
		}
	}
	internal bool SearchForSuccessionWarCities(City city){
		for(int i = 0; i < this.successionWarCities.Count; i++){
			if(this.successionWarCities[i].city.id == city.id){
				return true;
			}
		}
		return false;
	}
	internal bool SearchForInternationalWarCities(City city){
		for(int i = 0; i < this.intlWarCities.Count; i++){
			if(this.intlWarCities[i].city.id == city.id){
				return true;
			}
		}
		return false;
	}
	internal bool SearchForCivilWarCities(City city){
		for(int i = 0; i < this.civilWarCities.Count; i++){
			if(this.civilWarCities[i].city.id == city.id){
				return true;
			}
		}
		return false;
	}
	internal bool SearchForDefenseWarCities(City city, WAR_TYPE warType){
		for(int i = 0; i < this.defenseWarCities.Count; i++){
			if(this.defenseWarCities[i].city.id == city.id && this.defenseWarCities[i].warType == warType){
				return true;
			}
		}
		return false;
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
	internal void CampaignDone(Campaign doneCampaign, bool canCreate = true){
//		Campaign doneCampaign = SearchCampaignByID (campaign.id);
		EventManager.Instance.onWeekEnd.RemoveListener (doneCampaign.CheckExpiration);

		if(doneCampaign.isGhost){
			Debug.Log ("Ghost Campaign Done " + doneCampaign.campaignType.ToString ());
		}else{
			Debug.Log ("Campaign Done " + doneCampaign.campaignType.ToString () + " " + doneCampaign.targetCity.name);
			for(int i = 0; i < doneCampaign.registeredGenerals.Count; i++){
				doneCampaign.registeredGenerals[i].UnregisterThisGeneral (null, true, true);
			}
//			if(doneCampaign.registeredGenerals.Count > 0){
//				if(doneCampaign.campaignType == CAMPAIGN.OFFENSE){
//					if(doneCampaign.AreAllGeneralsOnRallyPoint()){
//						Debug.Log ("Will attack city now " + doneCampaign.targetCity.name);
//						doneCampaign.AttackCityNow ();
//					}
//				}else if(doneCampaign.campaignType == CAMPAIGN.DEFENSE){
//					if(doneCampaign.AreAllGeneralsOnRallyPoint()){
//						Debug.Log ("ALL GENERALS ARE ON DEFENSE CITY " + doneCampaign.targetCity.name + ". START EXPIRATION.");
//						doneCampaign.expiration = Utilities.defaultCampaignExpiration;
//					}
//				}
//			}

			this.MakeCityInactive (doneCampaign);
		}
		this.activeCampaigns.Remove (doneCampaign);
		doneCampaign = null;
		if(canCreate){
			CreateCampaign ();
		}
	}
	internal void UnregisterGenerals(General general, Campaign chosenCampaign){
		general.UnregisterThisGeneral (null, true, true);
//		if(general.targetLocation != null){
//			if(general.targetLocation.isOccupied){
//				general.targetLocation.city.incomingGenerals.Remove (general);
//			}
//		}
//		general.targetLocation = null;
//		general.warLeader = null;
//		general.campaignID = 0;
//		general.assignedCampaign = null;
//		general.targetCity = null;
//		general.rallyPoint = null;
//		general.daysBeforeArrival = 0;
//		general.RerouteToHome();

//		chosenCampaign.registeredGenerals.Remove (general);
	}
	internal HexTile GetRallyPoint(Campaign campaign){
		HexTile nearestHextile = GetHextile(campaign.targetCity);
		if(nearestHextile != null){
			return nearestHextile;
		}
		return null;
	}
	internal HexTile GetHextile(City targetCity){
		if(targetCity == null){
			return null;
		}
		List<HexTile> tilesInRange = targetCity.hexTile.GetTilesInRange (10f).Where(x => x.elevationType != ELEVATION.WATER).ToList();
		if(tilesInRange.Count > 0){
			return tilesInRange [UnityEngine.Random.Range (0, tilesInRange.Count)];
		}else{
			return null;
		}
//		List<City> eligibleCities = new List<City>();
//		for(int i = 0 ; i < targetCity.hexTile.connectedTiles.Count; i++){
//			if(targetCity.hexTile.connectedTiles[i].isOccupied){
//				City chosenCity = this.leader.city.kingdom.SearchForCityById (targetCity.hexTile.connectedTiles [i].city.id);
//				if(chosenCity != null){
//					eligibleCities.Add(chosenCity);
//				}
//			}
//		}
//
//		if(eligibleCities.Count > 0){
//			City nearest = eligibleCities[0];
//			Debug.Log (nearest.name + " NEAREST");
//			for(int i = 1; i < eligibleCities.Count; i++){
//				Debug.Log ("GETTING PATH FOR " + eligibleCities [i].name);
//				if(nearest != null){
//					Debug.Log ("GETTING PATH FOR " + nearest.name);
//				}else{
//					Debug.Log ("GETTING PATH FOR null nearest");
//				}
//
//				List<HexTile> path1 = PathGenerator.Instance.GetPath(eligibleCities[i].hexTile, targetCity.hexTile, PATHFINDING_MODE.COMBAT);
//				List<HexTile> path2 = PathGenerator.Instance.GetPath(nearest.hexTile, targetCity.hexTile, PATHFINDING_MODE.COMBAT);
//
//				if(path1 == null || path2 == null){
//					if (path1 != null && path2 == null) {
//						Debug.Log ("NO PATH FOR NEAREST " + nearest.name);
//						nearest = eligibleCities [i];
//					}else if (path1 == null && path2 != null) {
//						Debug.Log ("NO PATH FOR ELIGIBLE CITY " + eligibleCities[i].name);
//					}else if(path1 == null && path2 == null){
//						Debug.Log ("NO PATH FOR BOTH " + eligibleCities[i].name + " and " + nearest.name);
//						nearest = null;
//					}
//				}else{
//					Debug.Log ("HAS PATH FOR BOTH " + eligibleCities[i].name + " and " + nearest.name);
//					if(path1.Count < path2.Count){
//						nearest = eligibleCities[i];
//					}
//				}
//			}
//			if(nearest != null){
//				Debug.Log (nearest.name + " NEAREST");
//			}else{
//				Debug.Log ("null NEAREST");
//			}
//			return nearest;
//		}else{
//			return null;
//		}
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
//					return CAMPAIGN.OFFENSE;
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

		this.intlWarCities.RemoveAll (x => x.city == null);
		this.civilWarCities.RemoveAll (x => x.city == null);
		this.successionWarCities.RemoveAll (x => x.city == null);
		this.defenseWarCities.RemoveAll (x => x.city == null);

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

//	internal void GeneralHasArrived(General general){
//		if (general.generalAvatar != null) {
//			GameObject.Destroy (general.generalAvatar);
//			general.generalAvatar = null;
//		}
//		general.targetLocation = null;
//		if (general.isGoingHome) {
//			if(general.citizen.isDead){
//				general.citizen.city.LookForNewGeneral (general);
//			}else{
//				general.citizen.city.LookForLostArmy (general);
//			}
//		} else {
//			Campaign chosenCampaign = general.assignedCampaign.leader.campaignManager.SearchCampaignByID(general.assignedCampaign.id);
//			if (chosenCampaign != null) {
//				if (chosenCampaign.campaignType == CAMPAIGN.OFFENSE) {
//					if (general.location == chosenCampaign.rallyPoint) {
//						Debug.Log (general.citizen.name + " has arrived at rally point " + chosenCampaign.rallyPoint.tileName);
//						if (chosenCampaign.AreAllGeneralsOnRallyPoint()) {
//							Debug.Log ("Will attack city now " + chosenCampaign.targetCity.name);
//							chosenCampaign.AttackCityNow();
//						}
//					} else if (general.location == chosenCampaign.targetCity.hexTile) {
//						//InitiateBattle
//						Debug.Log (general.citizen.name + " has arrived at target city " + chosenCampaign.targetCity.name);
//						CombatManager.Instance.CityBattle (chosenCampaign.targetCity);
////				((General)general.assignedRole).inAction = false;
//
//						//If army is alive but no general, after task immediately return home
////				if(general.isDead){
////					((General)general.assignedRole).UnregisterThisGeneral (chosenCampaign);
////				}
//					}
//				} else {
//					if (general.location == chosenCampaign.targetCity.hexTile) {
//						//InitiateDefense
////				((General)general.assignedRole).inAction = false;
//						Debug.Log (general.citizen.name + " has arrived at defense target city " + chosenCampaign.targetCity.name);
//						if (chosenCampaign.expiration == -1) {
//							if (chosenCampaign.AreAllGeneralsOnDefenseCity()) {
//								Debug.Log ("ALL GENERALS ARE ON DEFENSE CITY " + chosenCampaign.targetCity.name + ". START EXPIRATION.");
//								chosenCampaign.expiration = Utilities.defaultCampaignExpiration;
//							}
//						}
//					}
//				}
//			}
//		}
//		general.isGoingHome = false;
//	}
	internal Campaign SearchCampaignByID(int id){
		for(int i = 0; i < this.activeCampaigns.Count; i++){
			if(this.activeCampaigns[i].id == id){
				return this.activeCampaigns [i];
			}
		}
		return null;
	}
}
