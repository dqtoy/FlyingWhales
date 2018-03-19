using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomManager : MonoBehaviour {

	public static KingdomManager Instance = null;

    [SerializeField] private List<InitialKingdom> initialKingdomSetup;

	public List<Kingdom> allKingdoms = new List<Kingdom>();
	public List<Kingdom> kingdomRankings = new List<Kingdom>();
//	public Dictionary<int, int> kingdomRankings = new Dictionary<int, int>();

    public List<Kingdom> allKingdomsOrderedBy;
    [SerializeField] private ORDER_BY _orderKingdomsBy;

	public KingdomTypeData kingdomTypeNoble;
	public KingdomTypeData kingdomTypeEvil;
	public KingdomTypeData kingdomTypeMerchant;
	public KingdomTypeData kingdomTypeChaotic;

	public int initialSpawnRate;
	public int maxKingdomEventHistory;
	public int rangerMoveRange;
	public int evenActionDay;
    public int oddActionDay = 1;

	private List<AlliancePool> _alliances = new List<AlliancePool>();
	private List<Warfare> _kingdomWars = new List<Warfare>();
	private List<InternationalIncident> _internationalIncidents = new List<InternationalIncident>();

    private List<Warfare> _allWarsThatOccured = new List<Warfare>();

    [Space(10)]
    [Header("Kingdom Type Modifiers")]
    [SerializeField] internal float smallToMediumReqPercentage;
    [SerializeField] internal float mediumToLargeReqPercentage;
    [SerializeField] internal int smallToMediumReq;
    [SerializeField] internal int mediumToLargeReq;

    [Space(10)]
    [SerializeField] private int minimumInitialKingdomDistance;

    [SerializeField] private bool _useFogOfWar;

    [SerializeField] private List<Sprite> _emblemBGs;
    [SerializeField] private List<Sprite> _emblems;

    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();

    private Dictionary<int, List<Kingdom>> evenActionDays;
    private Dictionary<int, List<Kingdom>> oddActionDays;

    public const int KINGDOM_MAX_EXPANSION_RATE = 500;

    #region getters/setters
    public bool useFogOfWar {
        get { return this._useFogOfWar; }
    }
	public List<AlliancePool> alliances {
		get { return this._alliances; }
	}
	public List<Warfare> kingdomWars {
		get { return this._kingdomWars; }
	}
	public List<InternationalIncident> internationalIncidents {
		get { return this._internationalIncidents; }
	}
    public ORDER_BY orderKingdomsBy {
        get { return _orderKingdomsBy; }
    }
    public List<Warfare> allWarsThatOccured {
        get { return _allWarsThatOccured; }
    }
    #endregion

    void Awake(){
		Instance = this;
        evenActionDays = new Dictionary<int, List<Kingdom>>();
        for (int i = 2; i < 28; i+=2) {
            evenActionDays.Add(i, new List<Kingdom>());
        }
        oddActionDays = new Dictionary<int, List<Kingdom>>();
        for (int i = 1; i < 27; i += 2) {
            oddActionDays.Add(i, new List<Kingdom>());
        }
    }

    public void GenerateInitialKingdoms() {
        smallToMediumReq = Mathf.FloorToInt((float)GridMap.Instance.numOfRegions * (smallToMediumReqPercentage / 100f));
        mediumToLargeReq = Mathf.FloorToInt((float)GridMap.Instance.numOfRegions * (mediumToLargeReqPercentage / 100f));
        List<Region> allRegions = new List<Region>(GridMap.Instance.allRegions);
		//List<RESOURCE> initialResources = new List<RESOURCE>(){ RESOURCE.DEER, RESOURCE.PIG, RESOURCE.WHEAT, RESOURCE.RICE };
        for (int i = 0; i < initialKingdomSetup.Count; i++) {
            InitialKingdom initialKingdom = initialKingdomSetup[i];
            RACE initialKingdomRace = initialKingdom.race;
//			List<Region> regionsToChooseFrom = allRegions.Where(x => x.specialResource == RESOURCE.DEER || x.specialResource == RESOURCE.PIG || x.specialResource == RESOURCE.WHEAT || x.specialResource == RESOURCE.RICE).OrderByDescending(x => x.naturalResourceLevel[initialKingdomRace]).Take(Mathf.FloorToInt((float)GridMap.Instance.numOfRegions / 3f)).ToList();
			Region regionForKingdom = allRegions[Random.Range(0, allRegions.Count)];
            allRegions.Remove(regionForKingdom);
			RemoveAdjacentRegionsFrom (regionForKingdom, allRegions);

			//RESOURCE chosenResource = initialResources [UnityEngine.Random.Range (0, initialResources.Count)];
			//regionForKingdom.SetSpecialResource (chosenResource);
			//regionForKingdom.ComputeNaturalResourceLevel();
			//bool hasBeenRemoved = ResourcesManager.Instance.ReduceResourceCount (chosenResource);
			//if(hasBeenRemoved){
			//	initialResources.Remove (chosenResource);
			//}

            Kingdom newKingdom = GenerateNewKingdom(initialKingdomRace, new List<HexTile>() { regionForKingdom.centerOfMass }, true);
            newKingdom.HighlightAllOwnedTilesInKingdom();
        }
//		GameDate nextUpdateDate = new GameDate (GameManager.Instance.month, 1, GameManager.Instance.year);
//		nextUpdateDate.AddMonths (1);
//		SchedulingManager.Instance.AddEntry (nextUpdateDate, () => MonthlyUpdateKingdomRankings ());
    }
	private void RemoveAdjacentRegionsFrom(Region region, List<Region> allRegions){
		for (int i = 0; i < region.adjacentRegions.Count; i++) {
			allRegions.Remove (region.adjacentRegions [i]);
		}
	}
	public Kingdom GenerateNewKingdom(RACE race, List<HexTile> cities, bool createFamilies = false, Kingdom sourceKingdom = null, bool broadcastCreation = true){
		Kingdom newKingdom = new Kingdom (race, cities, sourceKingdom); //Create new kingdom
		AddKingdom(newKingdom);
        Debug.Log("Created new kingdom: " + newKingdom.name);
		newKingdom.militaryManager = new MilitaryManager2 (newKingdom);
        newKingdom.CreateInitialCities(cities); //Create initial cities

        if (createFamilies) { //create families?
            newKingdom.CreateInitialFamilies();
			for (int i = 0; i < newKingdom.cities.Count; i++) {
                City currCity = newKingdom.cities[i];
                currCity.hexTile.CreateCityNamePlate(currCity);
                currCity.SetupInitialValues();
				newKingdom.militaryManager.UpdateMaxGenerals ();
            }

        }

        //Create Relationships first
//        newKingdom.CreateInitialRelationships();
        if (broadcastCreation) {
            Messenger.Broadcast<Kingdom>("OnNewKingdomCreated", newKingdom);
        }
        newKingdom.UpdateAllRelationshipsLikeness();
        
        return newKingdom;
	}

    //public Kingdom SplitKingdom(Kingdom sourceKingdom, List<City> citiesToSplit, Citizen king) {
    //    Kingdom newKingdom = GenerateNewKingdom(sourceKingdom.race, new List<HexTile>() { }, false, sourceKingdom, false);
    //    //assign king if any
    //    if (king != null) {
    //        newKingdom.AssignNewKing(king);
    //    }
    //    Messenger.Broadcast<Kingdom>("OnNewKingdomCreated", newKingdom);
    //    TransferCitiesToOtherKingdom(sourceKingdom, newKingdom, citiesToSplit);
    //    return newKingdom;
    //}

    public void TransferCitiesToOtherKingdom(Kingdom sourceKingdom, Kingdom otherKingdom, List<City> citiesToTransfer) {
        //sourceKingdom.UnHighlightAllOwnedTilesInKingdom();
        for (int i = 0; i < citiesToTransfer.Count; i++) {
            City currCity = citiesToTransfer[i];
            List<Citizen> remainingCitizens = sourceKingdom.RemoveCityFromKingdom(currCity);
            //otherKingdom.AddCityToKingdom(currCity);
            currCity.ChangeKingdom(otherKingdom, remainingCitizens);
            //currCity.hexTile.ShowCitySprite();
            //currCity.hexTile.ShowNamePlate();
        }
		KingdomManager.Instance.UpdateKingdomList();
    }
		

	public void DeclarePeaceBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
//		KingdomRelationship kingdom1Rel = kingdom1.GetRelationshipWithKingdom(kingdom2);
//		KingdomRelationship kingdom2Rel = kingdom2.GetRelationshipWithKingdom(kingdom1);
//
//		kingdom1Rel.SetWarStatus(false);
//		kingdom2Rel.SetWarStatus(false);

//		kingdom1.AdjustExhaustionToAllRelationship (-15);
//		kingdom2.AdjustExhaustionToAllRelationship (-15);

		//kingdom1.UpdateAllGovernorsLoyalty ();
		//kingdom2.UpdateAllGovernorsLoyalty ();

//		kingdom1.RemoveInternationalWar(kingdom2);
//		kingdom2.RemoveInternationalWar(kingdom1);
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

    public List<Citizen> GetAllKings() {
        List<Citizen> kings = new List<Citizen>();
        for (int i = 0; i < allKingdoms.Count; i++) {
            kings.Add(allKingdoms[i].king);
        }
        return kings;
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
            if (currTileOfKingdom1.visibleByKingdoms.Contains(kingdom2)) {
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
				//if(governor.loyalty <= -25 && !governor.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.HONOR)){
				//	List<CHARACTER_VALUE> values = new List<CHARACTER_VALUE>(declarerKingdom.king.importantCharacterValues.Keys);
				//	for (int j = 0; j < values.Count; j++) {
				//		if(governor.citizen.importantCharacterValues.ContainsKey(values[j])){
				//			hasSameValues = true;
				//			break;
				//		}
				//	}

				//	if(hasSameValues){
				//		TransferCitiesToOtherKingdom (targetKingdom, declarerKingdom, targetKingdom.cities [i]);
				//		break;
				//	}
				//}
			}
		}
	}
	#endregion

	internal void DiscoverKingdom(Kingdom discovererKingdom, Kingdom discoveredKingdom){
        if(discovererKingdom.isDead || discoveredKingdom.isDead) {
            throw new System.Exception(discoveredKingdom.name + " - " + discoveredKingdom.isDead.ToString() + "/" + discovererKingdom.name + " - " + discovererKingdom.isDead.ToString());
        }
		if(!discovererKingdom.discoveredKingdoms.Contains(discoveredKingdom)){
			EventCreator.Instance.CreateKingdomDiscoveryEvent (discovererKingdom, discoveredKingdom);
			discovererKingdom.DiscoverKingdom(discoveredKingdom);
			discoveredKingdom.DiscoverKingdom(discovererKingdom);

			KingdomRelationship kr = discovererKingdom.GetRelationshipWithKingdom (discoveredKingdom);
			kr.ChangeDiscovery (true);

			for (int i = 0; i < discoveredKingdom.cities.Count; i++) {
				Region otherRegion = discoveredKingdom.cities[i].region;
				if (discovererKingdom.regionFogOfWarDict[otherRegion] != FOG_OF_WAR_STATE.VISIBLE) {
					discovererKingdom.SetFogOfWarStateForRegion(otherRegion, FOG_OF_WAR_STATE.SEEN);
				}
			}

			for (int i = 0; i < discovererKingdom.cities.Count; i++) {
				Region otherRegion = discovererKingdom.cities[i].region;
				if(discoveredKingdom.regionFogOfWarDict[otherRegion] != FOG_OF_WAR_STATE.VISIBLE) {
					discoveredKingdom.SetFogOfWarStateForRegion(otherRegion, FOG_OF_WAR_STATE.SEEN);
				}
			}
		}
	}

    internal void UpdateKingdomList() {
        if(_orderKingdomsBy == ORDER_BY.NAME) {
            allKingdomsOrderedBy = allKingdoms.OrderByDescending(x => x.name).ToList();
        } else if (_orderKingdomsBy == ORDER_BY.POPULATION) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.population).ToList();
        } else if (_orderKingdomsBy == ORDER_BY.CITIES) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.cities.Count).ToList();
        } else if (_orderKingdomsBy == ORDER_BY.EXPANSION_RATE) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.currentExpansionRate).ToList();
        } else if (_orderKingdomsBy == ORDER_BY.WEAPONS) {
//            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.soldiersCount).ToList();
        } 
//		else if (_orderKingdomsBy == KINGDOMS_ORDERED_BY.ARMOR) {
//            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.effectiveDefense).ToList();
//        }
        //allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.cities.Count).ToList();
        //UIManager.Instance.UpdateKingdomSummary();
    }

    internal void SetOrderKingdomsBy(ORDER_BY orderedBy) {
        _orderKingdomsBy = orderedBy;
        UpdateKingdomList();
    }

    internal int GetEvenActionDay(Kingdom kingdom) {
        int actionDayWithLeastKingdoms = 2;
        int leastNumberOfKingdomsInAnActionDay = 999;
        foreach (KeyValuePair<int, List<Kingdom>> kvp in evenActionDays) {
            int key = kvp.Key;
            List<Kingdom> value = kvp.Value;
            if(value.Count < leastNumberOfKingdomsInAnActionDay) {
                leastNumberOfKingdomsInAnActionDay = value.Count;
                actionDayWithLeastKingdoms = key;
            }
        }
        evenActionDays[actionDayWithLeastKingdoms].Add(kingdom);
        return actionDayWithLeastKingdoms;
    }

    internal int GetOddActionDay(Kingdom kingdom) {
        int actionDayWithLeastKingdoms = 1;
        int leastNumberOfKingdomsInAnActionDay = 999;
        foreach (KeyValuePair<int, List<Kingdom>> kvp in oddActionDays) {
            int key = kvp.Key;
            List<Kingdom> value = kvp.Value;
            if (value.Count < leastNumberOfKingdomsInAnActionDay) {
                leastNumberOfKingdomsInAnActionDay = value.Count;
                actionDayWithLeastKingdoms = key;
            }
        }
        oddActionDays[actionDayWithLeastKingdoms].Add(kingdom);
        return actionDayWithLeastKingdoms;
    }

    internal void UnregisterKingdomFromActionDays(Kingdom kingdom) {
        evenActionDays[kingdom.actionDay].Remove(kingdom);
        //oddActionDays[kingdom.oddActionDay].Remove(kingdom);
    }

	internal void IncrementEvenActionDay(int value){
		this.evenActionDay += value;
	}
    internal void IncrementOddActionDay() {
        oddActionDay += 2;
    }
    #region For Testing
    //[ContextMenu("Test Split Kingdom")]
    //public void TestSplitKingdom() {
    //    Kingdom sourceKingdom = this.allKingdoms.FirstOrDefault();
    //    List<City> citiesToSplit = new List<City>() { sourceKingdom.cities.Last() };
    //    SplitKingdom(sourceKingdom, citiesToSplit, null);
    //    Messenger.Broadcast("UpdateUI");
    //}
    #endregion

	#region Alliance
	internal bool AttemptToCreateAllianceBetweenTwoKingdoms(Kingdom firstKingdom, Kingdom secondKingdom){
		KingdomRelationship krFirst = firstKingdom.GetRelationshipWithKingdom(secondKingdom);
		KingdomRelationship krSecond = secondKingdom.GetRelationshipWithKingdom(firstKingdom);
		//if(krFirst.totalLike >= 1 && krSecond.totalLike >= 1){
			AlliancePool newAlliance = new AlliancePool();
			newAlliance.AddKingdomInAlliance(firstKingdom);
			newAlliance.AddKingdomInAlliance(secondKingdom);
			AddAlliancePool (newAlliance);
			Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Alliance", "create_alliance");
			newLog.AddToFillers (firstKingdom, firstKingdom.name, LOG_IDENTIFIER.FACTION_1);
			newLog.AddToFillers (secondKingdom, secondKingdom.name, LOG_IDENTIFIER.FACTION_2);
            //newLog.AddToFillers(null, newAlliance.name, LOG_IDENTIFIER.PARTY_NAME);
			newLog.AddAllInvolvedObjects (newAlliance.kingdomsInvolved.ToArray ());
			UIManager.Instance.ShowNotification (newLog);
			return true;
		//}
		//return false;
	}
	internal void AddAlliancePool(AlliancePool alliancePool){
		this._alliances.Add (alliancePool);
	}
	internal void RemoveAlliancePool(AlliancePool alliancePool){
		this._alliances.Remove (alliancePool);
	}
	#endregion

	#region Warfare
	internal void AddWarfare(Warfare warfare){
		this._kingdomWars.Add (warfare);
        this._allWarsThatOccured.Add(warfare);
    }
	internal void RemoveWarfare(Warfare warfare){
		this._kingdomWars.Remove (warfare);
	}
	#endregion

	#region International Incidents
	internal void AddInternationalIncidents(InternationalIncident internationalIncident){
		this._internationalIncidents.Add (internationalIncident);
	}
	internal void RemoveInternationalIncidents(InternationalIncident internationalIncident){
		this._internationalIncidents.Remove (internationalIncident);
	}
	#endregion

	internal int GetReducedInvasionValueThreshHold(float originalValue, int overPopulation){
		int newValue = (int)(originalValue - (float)overPopulation);
		if(newValue < 1){
			newValue = 1;
		}
		return newValue;
	}
	internal void AddKingdom(Kingdom kingdom){
		this.allKingdoms.Add (kingdom);
//		this.kingdomRankings.Add (kingdom.id, 0);
		this.kingdomRankings.Add (kingdom);
//		UpdateKingdomRankings ();
	}
	internal void RemoveKingdom(Kingdom kingdom){
		this.allKingdoms.Remove (kingdom);
//		this.kingdomRankings.Remove (kingdom.id);
		this.kingdomRankings.Remove (kingdom);
//		UpdateKingdomRankings ();
	}
	internal void UpdateKingdomRankings(){
//		int highestEffectiveKingdom = 0;
//		for (int i = 0; i < this.allKingdoms.Count; i++) {
//			int effectiveAttDef = this.allKingdoms [i].effectiveAttack + this.allKingdoms [i].effectiveDefense;
//			if(effectiveAttDef > highestEffectiveKingdom){
//				highestEffectiveKingdom = effectiveAttDef;
//			}
//		}
		this.kingdomRankings = this.kingdomRankings.OrderByDescending (x => x.effectiveAttack).ToList ();
	}
	private void MonthlyUpdateKingdomRankings(){
		if(this.kingdomRankings.Count > 1){
			UpdateKingdomRankings ();

			GameDate nextUpdateDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			nextUpdateDate.AddMonths (1);
			SchedulingManager.Instance.AddEntry (nextUpdateDate, () => MonthlyUpdateKingdomRankings ());
		}
	}

	internal void StartUndeadKingdom(HexTile hexTile, int undeadCount){
		Kingdom newKingdom = GenerateNewKingdom(RACE.UNDEAD, new List<HexTile>() { hexTile });
		newKingdom.InitializeUndeadKingdom (undeadCount);
		newKingdom.HighlightAllOwnedTilesInKingdom();
	}

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal Sprite GenerateKingdomEmblem(Kingdom kingdom) {
        List<Sprite> emblemsToUse = new List<Sprite>(_emblems);
        for (int i = 0; i < emblemsToUse.Count; i++) {
            Sprite currSprite = emblemsToUse[i];
            if (!usedEmblems.Contains(currSprite)) {
                AddEmblemAsUsed(currSprite);
                return currSprite;
            }
        }
        throw new System.Exception("There are no more emblems for kingdom: " + kingdom.name);
    }
    internal Sprite GenerateKingdomEmblemBG() {
        return _emblemBGs[Random.Range(0, _emblemBGs.Count)];
    }
    internal void AddEmblemAsUsed(Sprite emblem) {
        if (!usedEmblems.Contains(emblem)) {
            usedEmblems.Add(emblem);
        } else {
            throw new System.Exception("Emblem " + emblem.name + " is already being used!");
        }
    }
    internal void RemoveEmblemAsUsed(Sprite emblem) {
        usedEmblems.Remove(emblem);
    }
    #endregion

    #region Trading
    internal List<TradeDeal> allTradeDeals = new List<TradeDeal>();
    internal void CreateTradeDeal(Kingdom kingdom1, Kingdom kingdom2) {
        kingdom1.AddTradeDealWith(kingdom2);
        kingdom2.AddTradeDealWith(kingdom1);
        allTradeDeals.Add(new TradeDeal(kingdom1, kingdom2));
        if (UIManager.Instance.goAlliance.activeSelf) {
            if (UIManager.Instance.warAllianceState == "alliance") {
                UIManager.Instance.UpdateAllianceSummary();
            }
        }

    }
    internal void RemoveTradeDeal(Kingdom kingdom1, Kingdom kingdom2) {
        kingdom1.RemoveTradeDealWith(kingdom2);
        kingdom2.RemoveTradeDealWith(kingdom1);
        for (int i = 0; i < allTradeDeals.Count; i++) {
            TradeDeal currDeal = allTradeDeals[i];
            if((kingdom1.id == currDeal.kingdom1.id && kingdom2.id == currDeal.kingdom2.id) || 
                (kingdom1.id == currDeal.kingdom2.id && kingdom2.id == currDeal.kingdom1.id)) {
                allTradeDeals.Remove(currDeal);
                break;
            }
        }
        if (UIManager.Instance.goAlliance.activeSelf) {
            if (UIManager.Instance.warAllianceState == "alliance") {
                UIManager.Instance.UpdateAllianceSummary();
            }
        }
    }
    #endregion
}
