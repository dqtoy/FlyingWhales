using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomManager : MonoBehaviour {

	public static KingdomManager Instance = null;

    [SerializeField] private List<InitialKingdom> initialKingdomSetup;

	public List<Kingdom> allKingdoms;

	public KingdomTypeData kingdomTypeBarbaric;
	public KingdomTypeData kingdomTypeHermit;
	public KingdomTypeData kingdomTypeReligious;
	public KingdomTypeData kingdomTypeOpportunistic;

	public KingdomTypeData kingdomTypeNoble;
	public KingdomTypeData kingdomTypeEvil;
	public KingdomTypeData kingdomTypeMerchant;
	public KingdomTypeData kingdomTypeChaotic;

	public KingdomTypeData kingdomTypeRighteous;
	public KingdomTypeData kingdomTypeWicked;

    protected const int UNREST_INCREASE_WAR = 10;

	public int initialSpawnRate;
	public int maxKingdomEventHistory;

    [SerializeField] private int minimumInitialKingdomDistance;

    [SerializeField] private bool _useDiscoveredKingdoms;
    [SerializeField] private bool _useFogOfWar;

    #region getters/setters
    public bool useDiscoveredKingdoms {
        get { return this._useDiscoveredKingdoms; }
    }
    public bool useFogOfWar {
        get { return this._useFogOfWar; }
    }
    #endregion

    void Awake(){
		Instance = this;
	}

	public void GenerateInitialKingdoms(List<HexTile> stoneHabitableTiles, List<HexTile> woodHabitableTiles) {

        List<HexTile> stoneElligibleTiles = new List<HexTile>(stoneHabitableTiles);
        stoneElligibleTiles = stoneElligibleTiles.Where(x => x.nearbyResourcesCount >= 3).ToList();

        List<HexTile> woodElligibleTiles = new List<HexTile>(woodHabitableTiles);
        woodElligibleTiles = woodElligibleTiles.Where(x => x.nearbyResourcesCount >= 3).ToList();

        for (int i = 0; i < initialKingdomSetup.Count; i++) {
            InitialKingdom initialKingdom = initialKingdomSetup[i];
            List<HexTile> tilesToChooseFrom = stoneElligibleTiles;
            if (Utilities.GetBasicResourceForRace(initialKingdom.race) == BASE_RESOURCE_TYPE.WOOD) {
				tilesToChooseFrom = woodElligibleTiles;
            }
			tilesToChooseFrom = tilesToChooseFrom.Where(x => x.biomeType == initialKingdom.startingBiome && !x.isOccupied).ToList();
            if (tilesToChooseFrom.Count <= 0) {
                continue;
            }

            List<HexTile> citiesForKingdom = new List<HexTile>();
            for (int j = 0; j < initialKingdom.numOfCities; j++) {
                if (tilesToChooseFrom.Count <= 0) {
                    break;
                }
                int chosenIndex = Random.Range(0, tilesToChooseFrom.Count);
                HexTile chosenHexTile = tilesToChooseFrom[chosenIndex];
                List<HexTile> nearHabitableTiles = chosenHexTile.GetTilesInRange(minimumInitialKingdomDistance).Where(x => x.isHabitable).ToList();
                citiesForKingdom.Add(chosenHexTile);
                tilesToChooseFrom.Remove(chosenHexTile);
                for (int k = 0; k < nearHabitableTiles.Count; k++) {
                    HexTile nearTile = nearHabitableTiles[k];
                    if (stoneElligibleTiles.Contains(nearTile)) {
                        stoneElligibleTiles.Remove(nearTile);
                    } else if (woodElligibleTiles.Contains(nearTile)) {
                        woodElligibleTiles.Remove(nearTile);
                    }
                }
            }
            if(citiesForKingdom.Count > 0) {
                Kingdom kingdom = GenerateNewKingdom(initialKingdom.race, citiesForKingdom, true);
				if(i == 0){
					UIManager.Instance.SetKingdomAsActive(KingdomManager.Instance.allKingdoms[0]);
				}
            }
        }

//		List<HexTile> elligibleTilesForHumans = new List<HexTile>();

//		for (int i = 0; i < stoneElligibleTiles.Count; i++) {
//			if (stoneElligibleTiles [i].nearbyResourcesCount > 3) {
//				elligibleTilesForHumans.Add (stoneElligibleTiles [i]);
//			}
//		}
////		Debug.Log ("Valid capital tiles: " + elligibleTilesForHumans.Count);

//		if (elligibleTilesForHumans.Count < this.numberOfKingdoms) {
//			this.numberOfKingdoms = elligibleTilesForHumans.Count;
//		}

//		for (int i = 0; i < this.numberOfKingdoms; i++) {
//			List<HexTile> citiesForKingdom = new List<HexTile>();
//			for (int j = 0; j < this.numberOfCitiesPerKingdom; j++) {
//                if (elligibleTilesForHumans.Count <= 0) {
//                    break;
//                }
//                int chosenIndex = Random.Range(0, elligibleTilesForHumans.Count);
//                HexTile chosenHexTile = elligibleTilesForHumans[chosenIndex];
//                List<HexTile> nearHabitableTiles = chosenHexTile.GetTilesInRange(minimumInitialKingdomDistance).Where(x => x.isHabitable).ToList();
//                citiesForKingdom.Add(elligibleTilesForHumans[chosenIndex]);
//                elligibleTilesForHumans.RemoveAt(chosenIndex);
//                for (int k = 0; k < nearHabitableTiles.Count; k++) {
//                    HexTile nearTile = nearHabitableTiles[k];
//                    if (elligibleTilesForHumans.Contains(nearTile)) {
//                        elligibleTilesForHumans.Remove(nearTile);
//                    }
//                }
//            }
			
//			GenerateNewKingdom (RACE.HUMANS, citiesForKingdom, true);
//		}

//		if (elligibleTilesForHumans.Count > 0) {
//			cityForHumans1.Add (elligibleTilesForHumans [0]);
////			cityForHumans1.Add (elligibleTilesForHumans [1]);
//			GenerateNewKingdom (RACE.HUMANS, cityForHumans1, true);
//		}
//
//		if (elligibleTilesForHumans.Count > 1) {
//			cityForHumans2.Add (elligibleTilesForHumans[1]);
////			cityForHumans2.Add (elligibleTilesForHumans[3]);
//			GenerateNewKingdom (RACE.HUMANS, cityForHumans2, true);
//		}
//
//		if (elligibleTilesForHumans.Count > 2) {
//			cityForHumans3.Add (elligibleTilesForHumans [2]);
////			cityForHumans3.Add (elligibleTilesForHumans [5]);
//			GenerateNewKingdom (RACE.HUMANS, cityForHumans3, true);
//		}
//
//		if (elligibleTilesForHumans.Count > 3) {
//			cityForHumans4.Add (elligibleTilesForHumans [3]);
//			GenerateNewKingdom (RACE.HUMANS, cityForHumans4, true);
//		}

//		for (int i = 0; i < elligibleTilesForHumans.Count; i++) {
//			habitableTiles.Remove (elligibleTilesForHumans[i]);
//		}

//		//Get Statrting City For Elves
//		List<HexTile> cityForElves = new List<HexTile>();
//		List<HexTile> elligibleTilesForElves = new List<HexTile>();
//		for (int i = 0; i < habitableTiles.Count; i++) {
//
//			List<HexTile> neighbours = habitableTiles[i].AllNeighbours.ToList();
//			List<HexTile> tilesContainingBaseResource = new List<HexTile>();
//			for (int j = 0; j < neighbours.Count; j++) {
//				if (neighbours[j].specialResource == RESOURCE.NONE) {
//					if (Utilities.GetBaseResourceType (neighbours[j].defaultResource) == BASE_RESOURCE_TYPE.WOOD) {
//						tilesContainingBaseResource.Add(neighbours[j]);
//					}
//				} else {
//					if (Utilities.GetBaseResourceType (neighbours[j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
//						tilesContainingBaseResource.Add(neighbours[j]);
//					}
//				}
//			}
//
//			if (tilesContainingBaseResource.Count > 0) {
//				elligibleTilesForElves.Add(habitableTiles[i]);
//			}
//		}
//		cityForElves.Add (elligibleTilesForElves [Random.Range (0, elligibleTilesForElves.Count)]);
//		GenerateNewKingdom (RACE.ELVES, cityForElves, true);
		CreateInitialRelationshipKings ();
	}

	internal void CreateInitialRelationshipKings(){
		for(int i = 0; i < this.allKingdoms.Count; i++){
			this.allKingdoms [i].king.CreateInitialRelationshipsToKings ();
		}
	}
	public Kingdom GenerateNewKingdom(RACE race, List<HexTile> cities, bool isForInitial = false, Kingdom sourceKingdom = null){
		Kingdom newKingdom = new Kingdom (race, cities, sourceKingdom);
		allKingdoms.Add(newKingdom);
		if (isForInitial) {
			for (int i = 0; i < cities.Count; i++) {
                HexTile currCityTile = cities[i];
				if (i == 0) {
                    currCityTile.city.CreateInitialFamilies();
				} else {
                    currCityTile.city.CreateInitialFamilies(false);
				}
                currCityTile.CreateCityNamePlate(currCityTile.city);
            }
		}
        //EventManager.Instance.onCreateNewKingdomEvent.Invoke(newKingdom);
        Messenger.Broadcast<Kingdom>("OnNewKingdomCreated", newKingdom);
        //this.UpdateKingdomAdjacency();
        newKingdom.CheckForDiscoveredKingdoms();
		return newKingdom;
	}

    public Kingdom SplitKingdom(Kingdom sourceKingdom, List<City> citiesToSplit) {
        Kingdom newKingdom = GenerateNewKingdom(sourceKingdom.race, new List<HexTile>() { }, false, sourceKingdom);
        TransferCitiesToOtherKingdom(sourceKingdom, newKingdom, citiesToSplit);
        return newKingdom;
    }

    public void TransferCitiesToOtherKingdom(Kingdom sourceKingdom, Kingdom otherKingdom, List<City> citiesToTransfer) {
        sourceKingdom.UnHighlightAllOwnedTilesInKingdom();
        for (int i = 0; i < citiesToTransfer.Count; i++) {
            City currCity = citiesToTransfer[i];
            sourceKingdom.RemoveCityFromKingdom(currCity);
            //otherKingdom.AddCityToKingdom(currCity);
            currCity.ChangeKingdom(otherKingdom);
            //currCity.hexTile.ShowCitySprite();
            //currCity.hexTile.ShowNamePlate();
        }
        
        if(UIManager.Instance.currentlyShowingKingdom.id == sourceKingdom.id) {
            sourceKingdom.HighlightAllOwnedTilesInKingdom();
        }
    }
	public void TransferCitiesToOtherKingdom(Kingdom sourceKingdom, Kingdom otherKingdom, City city) {
		sourceKingdom.UnHighlightAllOwnedTilesInKingdom();
		sourceKingdom.RemoveCityFromKingdom(city);
		//otherKingdom.AddCityToKingdom(currCity);
		city.ChangeKingdom(otherKingdom);
		//currCity.hexTile.ShowCitySprite();
		//currCity.hexTile.ShowNamePlate();

		if(UIManager.Instance.currentlyShowingKingdom.id == sourceKingdom.id) {
			sourceKingdom.HighlightAllOwnedTilesInKingdom();
		}
	}

	public void DeclareWarBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, War war){
		RelationshipKingdom kingdom1Rel = kingdom1.GetRelationshipWithOtherKingdom(kingdom2);
		RelationshipKingdom kingdom2Rel = kingdom2.GetRelationshipWithOtherKingdom(kingdom1);

        RelationshipKings king1Rel = kingdom1.king.GetRelationshipWithCitizen(kingdom2.king);
        RelationshipKings king2Rel = kingdom2.king.GetRelationshipWithCitizen(kingdom1.king);

        king1Rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.ENEMY, war);
        king2Rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.ENEMY, war);

        kingdom1Rel.SetWarStatus(true);
		kingdom2Rel.SetWarStatus(true);

		kingdom1Rel.kingdomWar.ResetKingdomWar ();
		kingdom2Rel.kingdomWar.ResetKingdomWar ();

		kingdom1.AdjustExhaustionToAllRelationship (15);
		kingdom2.AdjustExhaustionToAllRelationship (15);

		kingdom1.AddInternationalWar(kingdom2);
		kingdom2.AddInternationalWar(kingdom1);

        //kingdom1.RemoveAllTradeRoutesWithOtherKingdom(kingdom2);
        //kingdom2.RemoveAllTradeRoutesWithOtherKingdom(kingdom1);

        kingdom1.AdjustUnrest(UNREST_INCREASE_WAR);
        kingdom2.AdjustUnrest(UNREST_INCREASE_WAR);

		kingdom1.ActivateBoonOfPowers ();
		kingdom2.ActivateBoonOfPowers ();

		kingdom1.UpdateAllGovernorsLoyalty ();
		kingdom2.UpdateAllGovernorsLoyalty ();

		king1Rel.UpdateLikeness (null);
		king2Rel.UpdateLikeness (null);

//		war.UpdateWarPair ();
        //		kingdom1.king.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, kingdom1.king.name + " of " + kingdom1.name + " declares war against " + kingdom2.name + ".", HISTORY_IDENTIFIER.NONE));
        //		kingdom2.king.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, kingdom1.king.name + " of " + kingdom1.name + " declares war against " + kingdom2.name + ".", HISTORY_IDENTIFIER.NONE));

        Log declareWarLog = war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "declare_war");
		declareWarLog.AddToFillers (kingdom1.king, kingdom1.king.name, LOG_IDENTIFIER.KING_1);
		declareWarLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);

		WarEvents (kingdom1, kingdom2);

		KingdomManager.Instance.CheckWarTriggerDeclareWar (kingdom1, kingdom2);
//		War newWar = new War(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, null, kingdom1, kingdom2, invasionPlanThatStartedWar);
	}

	public void DeclarePeaceBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
//		RelationshipKingdom kingdom1Rel = kingdom1.GetRelationshipWithOtherKingdom(kingdom2);
//		RelationshipKingdom kingdom2Rel = kingdom2.GetRelationshipWithOtherKingdom(kingdom1);
//
//		kingdom1Rel.SetWarStatus(false);
//		kingdom2Rel.SetWarStatus(false);

		kingdom1.AdjustExhaustionToAllRelationship (-15);
		kingdom2.AdjustExhaustionToAllRelationship (-15);

		kingdom1.UpdateAllGovernorsLoyalty ();
		kingdom2.UpdateAllGovernorsLoyalty ();

//		kingdom1.RemoveInternationalWar(kingdom2);
//		kingdom2.RemoveInternationalWar(kingdom1);
	}

	public War GetWarBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		List<GameEvent> allWars = EventManager.Instance.GetEventsOfType(EVENT_TYPES.KINGDOM_WAR).Where(x => x.isActive).ToList();
		for (int i = 0; i < allWars.Count; i++) {
			War currentWar = (War)allWars[i];
			if (currentWar.kingdom1.id == kingdom1.id) {
				if (currentWar.kingdom2.id == kingdom2.id) {
					return currentWar;
				}
			} else if (currentWar.kingdom2.id == kingdom1.id) {
				if (currentWar.kingdom1.id == kingdom2.id) {
					return currentWar;
				}
			}
		}
		return null;
	}

	public JoinWar GetJoinWarRequestBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		List<GameEvent> allJoinWarRequests = EventManager.Instance.GetEventsOfType(EVENT_TYPES.JOIN_WAR_REQUEST).Where(x => x.isActive).ToList();
		for (int i = 0; i < allJoinWarRequests.Count; i++) {
			JoinWar currentJoinWar = (JoinWar)allJoinWarRequests[i];
			if (currentJoinWar.startedByKingdom.id == kingdom1.id) {
				if (currentJoinWar.candidateForAlliance.city.kingdom.id == kingdom2.id) {
					return currentJoinWar;
				}
			} else if (currentJoinWar.startedByKingdom.id == kingdom1.id) {
				if (currentJoinWar.candidateForAlliance.city.kingdom.id == kingdom2.id) {
					return currentJoinWar;
				}
			}
		}
		return null;
	}

	public RequestPeace GetRequestPeaceBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		List<GameEvent> allPeaceRequestsPerKingdom = EventManager.Instance.GetEventsStartedByKingdom(kingdom1, new EVENT_TYPES[]{EVENT_TYPES.REQUEST_PEACE});
		for (int i = 0; i < allPeaceRequestsPerKingdom.Count; i++) {
			RequestPeace currentRequestPeace = (RequestPeace)allPeaceRequestsPerKingdom[i];
			if (currentRequestPeace.startedByKingdom.id == kingdom1.id && currentRequestPeace.targetKingdom.id == kingdom2.id) {
				return currentRequestPeace;
			}
		}
		return null;
	}

	public void AddRelationshipToOtherKings(Citizen newKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != newKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.Add (new RelationshipKings(this.allKingdoms[i].king, newKing, 0));
			}
		}
	}
	public void RemoveRelationshipToOtherKings(Citizen oldKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != oldKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.RemoveAll (x => x.king.id == oldKing.id);
			}
		}
	}

	//public void MakeKingdomDead(Kingdom kingdomToDie){
	//	this.allKingdoms.Remove(kingdomToDie);
	//	RemoveRelationshipToOtherKingdoms (kingdomToDie);
	//}

	public void RemoveRelationshipToOtherKingdoms(Kingdom kingdomToRemove){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			for (int j = 0; j < this.allKingdoms[i].relationshipsWithOtherKingdoms.Count; j++) {
				if (this.allKingdoms[i].relationshipsWithOtherKingdoms[j].targetKingdom.id == kingdomToRemove.id) {
					this.allKingdoms[i].relationshipsWithOtherKingdoms.RemoveAt(j);
					break;
				}
			}
		}
	}

	public void UpdateKingdomAdjacency(){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			Kingdom currentKingdom = this.allKingdoms[i];
			currentKingdom.adjacentCitiesFromOtherKingdoms.Clear();
			currentKingdom.ResetAdjacencyWithOtherKingdoms();
			for (int j = 0; j < currentKingdom.cities.Count; j++) {
				City currentCity = currentKingdom.cities[j];
				for (int k = 0; k < currentCity.hexTile.connectedTiles.Count; k++) {
					HexTile currentConnectedTile = currentCity.hexTile.connectedTiles[k];
					if (currentConnectedTile.isOccupied && currentConnectedTile.city != null) {
						if (currentConnectedTile.city.kingdom.id != currentKingdom.id) {
							currentKingdom.GetRelationshipWithOtherKingdom(currentConnectedTile.city.kingdom).SetAdjacency(true);
							currentConnectedTile.city.kingdom.GetRelationshipWithOtherKingdom(currentKingdom).SetAdjacency(true);
							currentKingdom.adjacentCitiesFromOtherKingdoms.Add(currentConnectedTile.city);
						}
					}

				}
			}
			currentKingdom.adjacentCitiesFromOtherKingdoms.Distinct();
		}
	}

	public List<Kingdom> GetOtherKingdomsExcept(Kingdom kingdom){
		List<Kingdom> newKingdoms = new List<Kingdom> ();
		for(int i = 0; i < this.allKingdoms.Count; i++){
			if(this.allKingdoms[i].id != kingdom.id){
				newKingdoms.Add (this.allKingdoms [i]);
			}
		}
//		if(newKingdoms.Count > 0){
//			return newKingdoms [UnityEngine.Random.Range (0, newKingdoms.Count)];
//		}
		return newKingdoms;
	}

	// Counts the number of kingdoms of a specific type
	public int CountKingdomOfType(KINGDOM_TYPE kingdomType) {
		int count = 0;
		// Loop through the list of all kingdoms, filtering out dead kingdoms
		for(int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].isAlive() && this.allKingdoms[i].kingdomType == kingdomType) {
				count++;
			}
		}

		return count;
	}

	internal void CheckWarTriggerDeclareWar(Kingdom warDeclarer, Kingdom warReceiver){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if(this.allKingdoms[i].id != warDeclarer.id && this.allKingdoms[i].id != warReceiver.id){
				RelationshipKings relationshipToAffected = this.allKingdoms [i].king.GetRelationshipWithCitizen (warReceiver.king);
				RelationshipKings relationshipToTarget = this.allKingdoms [i].king.GetRelationshipWithCitizen (warDeclarer.king);

				if(relationshipToAffected.lordRelationship == RELATIONSHIP_STATUS.ALLY){
					this.allKingdoms [i].king.WarTrigger (relationshipToTarget, null, this.allKingdoms [i].kingdomTypeData, WAR_TRIGGER.TARGET_DECLARED_WAR_AGAINST_ALLY);
				}else if(relationshipToAffected.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
					this.allKingdoms [i].king.WarTrigger (relationshipToTarget, null, this.allKingdoms [i].kingdomTypeData, WAR_TRIGGER.TARGET_DECLARED_WAR_AGAINST_FRIEND);
				}
			}
		}
	}

	internal void CheckWarTriggerMisc(Kingdom targetKingdom, WAR_TRIGGER warTrigger){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms [i].id != targetKingdom.id) {
				RelationshipKings relationshipToTarget = this.allKingdoms [i].king.GetRelationshipWithCitizen (targetKingdom.king);
				this.allKingdoms [i].king.WarTrigger (relationshipToTarget, null, this.allKingdoms [i].kingdomTypeData, warTrigger);
			}
		}
	}

    private void UpdateDiscoveredKingdomsForAll() {
        for (int i = 0; i < this.allKingdoms.Count; i++) {
            this.allKingdoms[i].CheckForDiscoveredKingdoms();
        }
    }

    public void InheritRelationshipFromCitizen(Citizen citizenInheritedFrom, Citizen citizenToInherit) {
        citizenToInherit.relationshipKings = new List<RelationshipKings>(citizenInheritedFrom.relationshipKings);
        for (int i = 0; i < citizenToInherit.relationshipKings.Count; i++) {
            RelationshipKings currRel = citizenToInherit.relationshipKings[i];
            RelationshipKings relOfTargetKing = currRel.king.GetRelationshipWithCitizen(citizenInheritedFrom);
            currRel.ChangeSourceKing(citizenToInherit);
			relOfTargetKing.ChangeTargetKing(citizenToInherit);
        }
    }

    public List<Citizen> GetAllCitizensOfType(ROLE role) {
        List<Citizen> citizensOfType = new List<Citizen>();
        for (int i = 0; i < allKingdoms.Count; i++) {
            citizensOfType.AddRange(allKingdoms[i].GetAllCitizensOfType(role));
        }
        return citizensOfType;
    }

    public List<Kingdom> GetAllKingdomsByRace(RACE race) {
        List<Kingdom> kingdomsOfRace = new List<Kingdom>();
        for (int i = 0; i < allKingdoms.Count; i++) {
            Kingdom currKingdom = allKingdoms[i];
            if(currKingdom.race == race) {
                kingdomsOfRace.Add(currKingdom);
            }
        }
        return kingdomsOfRace;
    }

    #region For Testing

    [ContextMenu("Test Split Kingdom")]
    public void TestSplitKingdom() {
		Kingdom sourceKingdom = this.allKingdoms.FirstOrDefault();
        List<City> citiesToSplit = new List<City>() { sourceKingdom.cities.Last() };
        SplitKingdom(sourceKingdom, citiesToSplit);
        Messenger.Broadcast("UpdateUI");
    }
    #endregion

	internal void InstantWarBetweenKingdoms(Kingdom sourceKingdom, Kingdom targetKingdom, WAR_TRIGGER warTrigger, GameEvent gameEventTrigger = null){
		RelationshipKingdom relationship = sourceKingdom.GetRelationshipWithOtherKingdom (targetKingdom);
		if (relationship.war == null) {
			War newWar = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, 
				sourceKingdom, targetKingdom, warTrigger);
			newWar.DeclareWar (newWar.kingdom1);
			newWar.gameEventTrigger = gameEventTrigger;
		}
	}

    public bool IsSharingBorders(Kingdom kingdom1, Kingdom kingdom2) {
        List<HexTile> allTilesOfKingdom1 = new List<HexTile>();
        //List<HexTile> allTilesOfKingdom2 = new List<HexTile>();

        for (int i = 0; i < kingdom1.cities.Count; i++) {
            City currCity = kingdom1.cities[i];
            allTilesOfKingdom1 = allTilesOfKingdom1.Union(currCity.ownedTiles).ToList();
            allTilesOfKingdom1 = allTilesOfKingdom1.Union(currCity.borderTiles).ToList();
        }

        //for (int i = 0; i < kingdom2.cities.Count; i++) {
        //    City currCity = kingdom2.cities[i];
        //    allTilesOfKingdom2 = allTilesOfKingdom2.Union(currCity.ownedTiles).ToList();
        //    allTilesOfKingdom2 = allTilesOfKingdom2.Union(currCity.borderTiles).ToList();
        //}

        for (int i = 0; i < allTilesOfKingdom1.Count; i++) {
            HexTile currTileOfKingdom1 = allTilesOfKingdom1[i];
            if (currTileOfKingdom1.seenByKingdoms.Contains(kingdom2)) {
                return true;
            }
        }
        return false;
    }


	#region War Events
	private void WarEvents(Kingdom declarerKingdom, Kingdom targetKingdom){
		TriggerBackstabberEvent (declarerKingdom, targetKingdom);
	}
	private void TriggerBackstabberEvent(Kingdom declarerKingdom, Kingdom targetKingdom){
		bool hasSameValues = false;
		if(targetKingdom.cities.Count >= 3){
			for (int i = 0; i < targetKingdom.cities.Count; i++) {
				Governor governor = (Governor)targetKingdom.cities [i].governor.assignedRole;
				if(governor.loyalty <= -25 && !governor.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.HONOR)){
					List<CHARACTER_VALUE> values = new List<CHARACTER_VALUE>(declarerKingdom.king.importantCharacterValues.Keys);
					for (int j = 0; j < values.Count; j++) {
						if(governor.citizen.importantCharacterValues.ContainsKey(values[j])){
							hasSameValues = true;
							break;
						}
					}

					if(hasSameValues){
						TransferCitiesToOtherKingdom (targetKingdom, declarerKingdom, targetKingdom.cities [i]);
						break;
					}
				}
			}
		}
	}
	#endregion

	internal void DiscoverKingdom(Kingdom discovererKingdom, Kingdom discoveredKingdom){
		if(!discovererKingdom.discoveredKingdoms.Contains(discoveredKingdom)){
			EventCreator.Instance.CreateKingdomDiscoveryEvent (discovererKingdom, discoveredKingdom);
		}
		discovererKingdom.DiscoverKingdom(discoveredKingdom);
		discoveredKingdom.DiscoverKingdom(discovererKingdom);
	}
}
