using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public class KingdomManager : MonoBehaviour {

	public static KingdomManager Instance = null;

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

	public int numberOfKingdoms;
	public int numberOfCitiesPerKingdom;
	public int initialSpawnRate;

    public bool useDiscoveredKingdoms = true;

	void Awake(){
		Instance = this;
	}

	public void GenerateInitialKingdoms(List<HexTile> stoneElligibleTiles) {

		//Get Starting City For Humans
		List<HexTile> cityForHumans1 = new List<HexTile>();
		List<HexTile> cityForHumans2 = new List<HexTile>();
		List<HexTile> cityForHumans3 = new List<HexTile>();
		List<HexTile> cityForHumans4 = new List<HexTile>();

		List<HexTile> elligibleTilesForHumans = new List<HexTile>();

		for (int i = 0; i < stoneElligibleTiles.Count; i++) {
			if (stoneElligibleTiles [i].nearbyResourcesCount > 3) {
				elligibleTilesForHumans.Add (stoneElligibleTiles [i]);
			}
		}
//		Debug.Log ("Valid capital tiles: " + elligibleTilesForHumans.Count);

//		int numOfKingdoms = 2;
		if (elligibleTilesForHumans.Count < this.numberOfKingdoms) {
			this.numberOfKingdoms = elligibleTilesForHumans.Count;
		}

//        int numOfCitiesPerKingdom = 1;
		for (int i = 0; i < this.numberOfKingdoms; i++) {
			List<HexTile> citiesForKingdom = new List<HexTile>();
			for (int j = 0; j < this.numberOfCitiesPerKingdom; j++) {
                if (elligibleTilesForHumans.Count <= 0) {
                    break;
                }
                int chosenIndex = Random.Range(0, elligibleTilesForHumans.Count);
                citiesForKingdom.Add(elligibleTilesForHumans[chosenIndex]);
                elligibleTilesForHumans.RemoveAt(chosenIndex);
            }
			
			GenerateNewKingdom (RACE.HUMANS, citiesForKingdom, true);
		}

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
	public Kingdom GenerateNewKingdom(RACE race, List<HexTile> cities, bool isForInitial = false){
		Kingdom newKingdom = new Kingdom (race, cities);
		allKingdoms.Add(newKingdom);
		EventManager.Instance.onCreateNewKingdomEvent.Invoke(newKingdom);
		if (isForInitial) {
			for (int i = 0; i < cities.Count; i++) {
				if (i == 0) {
					cities [i].city.CreateInitialFamilies();
				} else {
					cities [i].city.CreateInitialFamilies(false);
				}
			}
		}
		//this.UpdateKingdomAdjacency();
        newKingdom.CheckForDiscoveredKingdoms();
		return newKingdom;
	}

    public Kingdom SplitKingdom(Kingdom sourceKingdom, List<City> citiesToSplit) {
        Kingdom newKingdom = GenerateNewKingdom(sourceKingdom.race, new List<HexTile>() { });
        TransferCitiesToOtherKingdom(sourceKingdom, newKingdom, citiesToSplit);
        return newKingdom;
    }

    private void TransferCitiesToOtherKingdom(Kingdom sourceKingdom, Kingdom otherKingdom, List<City> citiesToTransfer) {
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

        kingdom1.RemoveAllTradeRoutesWithOtherKingdom(kingdom2);
        kingdom2.RemoveAllTradeRoutesWithOtherKingdom(kingdom1);

        kingdom1.AdjustUnrest(UNREST_INCREASE_WAR);
        kingdom2.AdjustUnrest(UNREST_INCREASE_WAR);

//		war.UpdateWarPair ();
        //		kingdom1.king.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, kingdom1.king.name + " of " + kingdom1.name + " declares war against " + kingdom2.name + ".", HISTORY_IDENTIFIER.NONE));
        //		kingdom2.king.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, kingdom1.king.name + " of " + kingdom1.name + " declares war against " + kingdom2.name + ".", HISTORY_IDENTIFIER.NONE));

        Log declareWarLog = war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "declare_war");
		declareWarLog.AddToFillers (kingdom1.king, kingdom1.king.name);
		declareWarLog.AddToFillers (kingdom2, kingdom2.name);

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
		List<GameEvent> allPeaceRequestsPerKingdom = EventManager.Instance.GetEventsStartedByKingdom(kingdom1, new EVENT_TYPES[]{EVENT_TYPES.REQUEST_PEACE}).Where(x => x.isActive).ToList();
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

	public void MakeKingdomDead(Kingdom kingdomToDie){
		this.allKingdoms.Remove(kingdomToDie);
		RemoveRelationshipToOtherKingdoms (kingdomToDie);
	}

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

    #region For Testing

    [ContextMenu("Test Split Kingdom")]
    public void TestSplitKingdom() {
        Kingdom sourceKingdom = this.allKingdoms.First();
        List<City> citiesToSplit = new List<City>() { sourceKingdom.cities.Last() };
        SplitKingdom(sourceKingdom, citiesToSplit);
        EventManager.Instance.onUpdateUI.Invoke();
    }
    #endregion
}
