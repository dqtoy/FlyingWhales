using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CampaignManager {
	public Citizen leader;
	public int campaignLimit;
	public List<Campaign> activeCampaigns;
	public List<CampaignCandidates> candidates;

	public List<CityWar> intlWarCities;
	public List<CityWar> civilWarCities;
	public List<CityWar> successionWarCities;
	public List<CityWar> defenseWarCities;

	public CampaignManager(Citizen leader){
		this.leader = leader;
		this.campaignLimit = leader.GetCampaignLimit ();
		this.activeCampaigns = new List<Campaign>();
		this.candidates = new List<CampaignCandidates> ();
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
					
					List<City> civilWarCities = this.civilWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
					List<City> successionWarCities = this.successionWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
					List<City> intlWarCities = this.intlWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();

					if(civilWarCities.Count <= 0 && successionWarCities.Count <= 0 && intlWarCities.Count <= 0){
						campaignType = CAMPAIGN.DEFENSE;
						List<City> defenseCities = this.defenseWarCities.Where(x => !x.isActive).Select (x => x.city).ToList ();
						if(defenseCities.Count > 0){
							int nearestArrival = -2;
							City selectedCity = null;
							List<City> priorityDefense = defenseCities.Where (x => x.GetIncomingAttackers ().Count > 0).ToList();
							if(priorityDefense.Count > 0){
								priorityDefense = priorityDefense.OrderBy (x => nearestArrival = x.GetIncomingAttackers ().Min (y => y.daysBeforeArrival)).ToList ();
								selectedCity = GetCityToDefend(priorityDefense, nearestArrival);
							}else{
								selectedCity = defenseCities [UnityEngine.Random.Range (0, defenseCities.Count)];
							}
							//						defenseCities = defenseCities.OrderBy(x => x.incomingGenerals.Where(y => y.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && y.assignedCampaign.targetCity.id == x.id).Min(y => (int?) y.daysBeforeArrival) ?? (GridMap.Instance.height*GridMap.Instance.width)).ToList();
							if(selectedCity != null){
								int neededArmy = selectedCity.GetTotalAttackerStrength (nearestArrival);
								if(neededArmy <= 0){
									neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
								}
								UpdateCandidates (selectedCity.hexTile);
								newCampaign = new Campaign (this.leader, selectedCity, campaignType, WAR_TYPE.NONE, neededArmy, nearestArrival + 1);

								this.activeCampaigns.Add (newCampaign);
								this.MakeCityActive (newCampaign);
							}
						}else{
							Debug.Log ("CANT CREATE ANYMORE CAMPAIGNS");
//							return;
						}
//						
					} else {
						WAR_TYPE warType = WAR_TYPE.NONE;
						City target = null;
						GetWarAndTarget (intlWarCities, civilWarCities, successionWarCities, ref warType, ref target);

						if(target != null){
							int neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
							HexTile rallyPoint = GetRallyPoint (target, this.leader.city);
							if(rallyPoint != null){
								UpdateCandidates (rallyPoint);
								newCampaign = new Campaign (this.leader, target, campaignType, warType, neededArmy);
								newCampaign.rallyPoint = rallyPoint;
								this.activeCampaigns.Add (newCampaign);
								this.MakeCityActive (newCampaign);
							}else{
								Debug.Log (this.leader.name + " NO RALLY POINT for target " + target.name);
							}
						}else{
							Debug.Log (this.leader.name + " NO TARGET!");
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
						int nearestArrival = -2;
						City selectedCity = null;
						List<City> priorityDefense = defenseCities.Where (x => x.GetIncomingAttackers ().Count > 0).ToList();
						if(priorityDefense.Count > 0){
							priorityDefense = priorityDefense.OrderBy (x => nearestArrival = x.GetIncomingAttackers ().Min (y => y.daysBeforeArrival)).ToList ();
							selectedCity = GetCityToDefend(priorityDefense, nearestArrival);
						}else{
							selectedCity = defenseCities [UnityEngine.Random.Range (0, defenseCities.Count)];
						}
//						defenseCities = defenseCities.OrderBy(x => x.incomingGenerals.Where(y => y.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && y.assignedCampaign.targetCity.id == x.id).Min(y => (int?) y.daysBeforeArrival) ?? (GridMap.Instance.height*GridMap.Instance.width)).ToList();
						if(selectedCity != null){
							int neededArmy = selectedCity.GetTotalAttackerStrength (nearestArrival);
							if(neededArmy <= 0){
								neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
							}
							UpdateCandidates (selectedCity.hexTile);
							newCampaign = new Campaign (this.leader, selectedCity, campaignType, WAR_TYPE.NONE, neededArmy, nearestArrival + 1);

							this.activeCampaigns.Add (newCampaign);
							this.MakeCityActive (newCampaign);
						}

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

							if(target != null){
								int neededArmy = (int)(this.defenseWarCities.Sum (x => x.city.GetCityArmyStrength ()) * 0.25f);
								HexTile rallyPoint = GetRallyPoint (target, this.leader.city);
								if(rallyPoint != null){
									UpdateCandidates (rallyPoint);
									newCampaign = new Campaign (this.leader, target, campaignType, warType, neededArmy);
									newCampaign.rallyPoint = rallyPoint;
									this.activeCampaigns.Add (newCampaign);
									this.MakeCityActive (newCampaign);
								}else{
									Debug.Log (this.leader.name + " NO RALLY POINT for target " + target.name);
								}
							}else{
								Debug.Log (this.leader.name + " NO TARGET!");
							}
						}
					}
				}
				if(newCampaign != null){
					Debug.Log ("Created Campaign " + newCampaign.campaignType.ToString () + " " + newCampaign.targetCity.name);
//					EventManager.Instance.onRegisterOnCampaign.Invoke (newCampaign);
					if(newCampaign.campaignType == CAMPAIGN.DEFENSE){
						Log newLogTitle = newCampaign.CreateNewLogForCampaign (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Campaign", "DefensiveCampaign", "event_title");
						newLogTitle.AddToFillers (newCampaign.targetCity, newCampaign.targetCity.name);

						Log newLog = newCampaign.CreateNewLogForCampaign (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Campaign", "DefensiveCampaign", "start");
						newLog.AddToFillers (newCampaign.leader, newCampaign.leader.name);
						newLog.AddToFillers (newCampaign.leader.city.kingdom, newCampaign.leader.city.kingdom.name);
						newLog.AddToFillers (newCampaign.targetCity, newCampaign.targetCity.name);
					}else{
						
					}

					newCampaign.RegisterGenerals (this.candidates);
//					GameManager.Instance.StartCoroutine(AssignGeneralsOnCampaign (newCampaign));
				}else{
					//Create Ghost Campaign
					newCampaign = new Campaign (this.leader, null, CAMPAIGN.NONE, WAR_TYPE.NONE, 0);
					newCampaign.isGhost = true;
					Debug.Log ("Created Ghost Campaign " + newCampaign.campaignType.ToString ());

					this.activeCampaigns.Add (newCampaign);
					this.MakeCityActive (newCampaign);
				}
			}

			CreateCampaign ();
		}
	}
	private void UpdateCandidates(HexTile targetLocation){
		this.candidates.Clear ();
		EventManager.Instance.onCheckGeneralEligibility.Invoke (this.leader, targetLocation);
	}
	private City GetCityToDefend(List<City> cityPool, int nearest){
		List<City> citiesThatCanBeDefended = new List<City> ();
		int totalStrength = 0;
		int totalAttackerStrength = 0;
		for(int i = 0; i < cityPool.Count; i++){
			this.candidates.Clear ();
			EventManager.Instance.onCheckGeneralEligibility.Invoke (this.leader, cityPool[i].hexTile);
			List<General> attackers = cityPool [i].GetIncomingAttackers ();
			totalAttackerStrength = 0;
			for (int j = 0; j < attackers.Count; j++) {
				if(attackers[i].daysBeforeArrival == nearest){
					totalAttackerStrength += attackers [i].GetArmyHP ();
				}
			}

			totalStrength = 0;
			for (int j = 0; j < this.candidates.Count; j++) {
				if(this.candidates[i].path.Count <= nearest){
					if(totalStrength < totalAttackerStrength){
						totalStrength += this.candidates [i].general.GetArmyHP ();
					}else{
						break;
					}
				}else{
					if(this.candidates[i].general.location == cityPool[i].hexTile){
						if(totalStrength < totalAttackerStrength){
							totalStrength += this.candidates [i].general.GetArmyHP ();
						}else{
							break;
						}
					}
				}
			}

			if(totalStrength >= totalAttackerStrength){
				citiesThatCanBeDefended.Add (cityPool [i]);
			}
		}

		if(citiesThatCanBeDefended.Count > 0){
			return citiesThatCanBeDefended [UnityEngine.Random.Range (0, citiesThatCanBeDefended.Count)];
		}else{
			return cityPool[UnityEngine.Random.Range(0, cityPool.Count)];
		}
	}
	private IEnumerator AssignGeneralsOnCampaign(Campaign newCampaign){
		yield return null;
		newCampaign.RegisterGenerals (this.candidates);
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
		
	internal void GetWarAndTarget(List<City> intlWarCities, List<City> civilWarCities, List<City> successionWarCities, ref WAR_TYPE warType, ref City target){
		if(intlWarCities.Count > 0 && civilWarCities.Count > 0 && successionWarCities.Count > 0){
			int chance = UnityEngine.Random.Range (0, 3);
			if(chance == 0){
				warType = WAR_TYPE.INTERNATIONAL;
				target = GetNearestCityFromLeader(intlWarCities);
			}else if(chance == 1){
				warType = WAR_TYPE.CIVIL;
				target = GetNearestCityFromLeader(civilWarCities);
			}else{
				warType = WAR_TYPE.SUCCESSION;
				target = GetNearestCityFromLeader(successionWarCities);
			}
		}else if(intlWarCities.Count > 0 && civilWarCities.Count > 0 && successionWarCities.Count <= 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				warType = WAR_TYPE.INTERNATIONAL;
				target = GetNearestCityFromLeader(intlWarCities);
			}else if(chance == 1){
				warType = WAR_TYPE.CIVIL;
				target = GetNearestCityFromLeader(civilWarCities);
			}
		}else if(intlWarCities.Count > 0 && civilWarCities.Count <= 0 && successionWarCities.Count > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				warType = WAR_TYPE.INTERNATIONAL;
				target = GetNearestCityFromLeader(intlWarCities);
			}else if(chance == 1){
				warType = WAR_TYPE.SUCCESSION;
				target = GetNearestCityFromLeader(successionWarCities);
			}
		}else if(intlWarCities.Count <= 0 && civilWarCities.Count > 0 && successionWarCities.Count > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				warType = WAR_TYPE.CIVIL;
				target = GetNearestCityFromLeader(civilWarCities);
			}else if(chance == 1){
				warType = WAR_TYPE.SUCCESSION;
				target = GetNearestCityFromLeader(successionWarCities);
			}
		}else if(intlWarCities.Count > 0 && civilWarCities.Count <= 0 && successionWarCities.Count <= 0){
			warType = WAR_TYPE.INTERNATIONAL;
			target = GetNearestCityFromLeader(intlWarCities);
		}else if(intlWarCities.Count <= 0 && civilWarCities.Count > 0 && successionWarCities.Count <= 0){
			warType = WAR_TYPE.CIVIL;
			target = GetNearestCityFromLeader(civilWarCities);
		}else if(intlWarCities.Count <= 0 && civilWarCities.Count <= 0 && successionWarCities.Count > 0){
			warType = WAR_TYPE.SUCCESSION;
			target = GetNearestCityFromLeader(successionWarCities);
		}else{
			warType = WAR_TYPE.NONE;
			target = null;
		}
	}
	internal City GetNearestCityFromLeader(List<City> cities){
		City nearestCity = null;
		int nearestDistance = 0;
		for(int i = 0; i < cities.Count; i++){
			List<HexTile> path = PathGenerator.Instance.GetPath (cities [i].hexTile, this.leader.city.hexTile, PATHFINDING_MODE.COMBAT);
			if(path != null){
				if(nearestCity == null){
					nearestCity = cities [i];
					nearestDistance = path.Count;
				}else{
					if(path.Count < nearestDistance){
						nearestCity = cities [i];
						nearestDistance = path.Count;
					}
				}
			}
		}
		return nearestCity;
	}
	internal void CampaignDone(Campaign doneCampaign, bool canCreate = true){
//		Campaign doneCampaign = SearchCampaignByID (campaign.id);
		doneCampaign.isDone = true;
		EventManager.Instance.onWeekEnd.RemoveListener (doneCampaign.CheckExpiration);
		if(doneCampaign.isGhost){
			Debug.Log ("Ghost Campaign Done " + doneCampaign.campaignType.ToString ());
		}else{
			Debug.Log ("Campaign Done " + doneCampaign.campaignType.ToString () + " " + doneCampaign.targetCity.name);
			for(int i = 0; i < doneCampaign.registeredGenerals.Count; i++){
				doneCampaign.registeredGenerals[i].UnregisterThisGeneral (true, true);
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
		general.UnregisterThisGeneral (true, true);
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
	internal HexTile GetRallyPoint(City targetCity, City sourceCity){
		HexTile nearestHextile = GetHextile(targetCity, sourceCity);
		if(nearestHextile != null){
			return nearestHextile;
		}
		return null;
	}
	internal HexTile GetHextile(City targetCity, City sourceCity){
		if(targetCity == null){
			return null;
		}
		List<HexTile> tilesInRange = targetCity.hexTile.GetTilesInRange (5).Where(x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable && x.HasCombatPathTo(targetCity.hexTile)).ToList();
		if(tilesInRange.Count > 0){
			HexTile nearestTile = null;
			float nearestDistance = 0f;
			for(int i = 0; i < tilesInRange.Count; i++){
				float distance = Vector3.Distance (tilesInRange [i].transform.position, sourceCity.hexTile.transform.position);
				if(nearestTile == null){
					nearestTile = tilesInRange [i];
					nearestDistance = distance;
				}else{
					if(distance < nearestDistance){
						nearestTile = tilesInRange [i];
						nearestDistance = distance;
					}
				}
			}
			if(nearestTile != null){
				return nearestTile;
			}else{
				return tilesInRange [UnityEngine.Random.Range (0, tilesInRange.Count)];
			}
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
	internal void AddCandidate(General general, List<HexTile> path){
		this.candidates.Add (new CampaignCandidates (general, path));
	}
//	internal int GetGeneralCountByPercentage(float percent){
//		int totalGenerals = 0;
//		for(int i = 0; i < this.controlledGovernorsAndKings.Count; i++){
//			if(this.controlledGovernorsAndKings[i].isGovernor){
//				for(int j = 0; j < this.controlledGovernorsAndKings[i].city.citizens.Count; j++){
//					if(this.controlledGovernorsAndKings[i].city.citizens[j].role == ROLE.GENERAL){
//						totalGenerals += 1;
//					}
//				}
//			}
//			if (this.controlledGovernorsAndKings [i].isKing) {
//				for(int j = 0; j < this.controlledGovernorsAndKings[i].city.kingdom.cities.Count; j++){
//					for(int k = 0; k < this.controlledGovernorsAndKings[i].city.kingdom.cities[j].citizens.Count; k++){
//						if (this.controlledGovernorsAndKings [i].city.kingdom.cities [j].citizens [k].role == ROLE.GENERAL) {
//							totalGenerals += 1;
//						}
//					}
//				}
//			}
//		}
//		float percentage = (percent / 100f) * totalGenerals;
//
//		return (int)percentage;
//	}

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
//	internal Campaign SearchCampaignByID(int id){
//		for(int i = 0; i < this.activeCampaigns.Count; i++){
//			if(this.activeCampaigns[i].id == id){
//				return this.activeCampaigns [i];
//			}
//		}
//		return null;
//	}
}
