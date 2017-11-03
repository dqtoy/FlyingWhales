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
    [SerializeField] private KINGDOMS_ORDERED_BY _orderKingdomsBy;

	public KingdomTypeData kingdomTypeNoble;
	public KingdomTypeData kingdomTypeEvil;
	public KingdomTypeData kingdomTypeMerchant;
	public KingdomTypeData kingdomTypeChaotic;

    protected const int STABILITY_DECREASE_WAR = -10;

	public int initialSpawnRate;
	public int maxKingdomEventHistory;
	public int rangerMoveRange;
	public int evenActionDay;
    public int oddActionDay = 1;

	private List<AlliancePool> _alliances = new List<AlliancePool>();
	private List<Warfare> _kingdomWars = new List<Warfare>();

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

    private Dictionary<int, List<Kingdom>> evenActionDays;
    private Dictionary<int, List<Kingdom>> oddActionDays;

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
    public KINGDOMS_ORDERED_BY orderKingdomsBy {
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

        for (int i = 0; i < initialKingdomSetup.Count; i++) {
            InitialKingdom initialKingdom = initialKingdomSetup[i];
            RACE initialKingdomRace = initialKingdom.race;
            List<Region> regionsToChooseFrom = allRegions.OrderByDescending(x => x.naturalResourceLevel[initialKingdomRace]).Take(Mathf.FloorToInt((float)GridMap.Instance.numOfRegions / 3f)).ToList();
            Region regionForKingdom = regionsToChooseFrom[Random.Range(0, regionsToChooseFrom.Count)];
            allRegions.Remove(regionForKingdom);
            Kingdom newKingdom = GenerateNewKingdom(initialKingdomRace, new List<HexTile>() { regionForKingdom.centerOfMass }, true);
            newKingdom.HighlightAllOwnedTilesInKingdom();
        }
        UIManager.Instance.SetKingdomAsActive(KingdomManager.Instance.allKingdoms[0]);

		GameDate nextUpdateDate = new GameDate (GameManager.Instance.month, 1, GameManager.Instance.year);
		nextUpdateDate.AddMonths (1);
		SchedulingManager.Instance.AddEntry (nextUpdateDate, () => MonthlyUpdateKingdomRankings ());
    }

	public Kingdom GenerateNewKingdom(RACE race, List<HexTile> cities, bool createFamilies = false, Kingdom sourceKingdom = null, bool broadcastCreation = true){
		Kingdom newKingdom = new Kingdom (race, cities, sourceKingdom); //Create new kingdom
		AddKingdom(newKingdom);
        Debug.Log("Created new kingdom: " + newKingdom.name);
        newKingdom.CreateInitialCities(cities); //Create initial cities

        if (createFamilies) { //create families?
            newKingdom.CreateInitialFamilies();
			for (int i = 0; i < newKingdom.cities.Count; i++) {
                City currCity = newKingdom.cities[i];
                currCity.hexTile.CreateCityNamePlate(currCity);
                currCity.SetupInitialValues();
            }
        }

        //if(newKingdom.king == null) {
        //    throw new System.Exception("New kingdom " + newKingdom.name + " has no king created on generation!\n" + System.Environment.StackTrace);
        //}

        //Create Relationships first
        newKingdom.CreateInitialRelationships();
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
    }
		

	public void DeclarePeaceBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
//		KingdomRelationship kingdom1Rel = kingdom1.GetRelationshipWithKingdom(kingdom2);
//		KingdomRelationship kingdom2Rel = kingdom2.GetRelationshipWithKingdom(kingdom1);
//
//		kingdom1Rel.SetWarStatus(false);
//		kingdom2Rel.SetWarStatus(false);

		kingdom1.AdjustExhaustionToAllRelationship (-15);
		kingdom2.AdjustExhaustionToAllRelationship (-15);

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
		
    private void UpdateDiscoveredKingdomsForAll() {
        for (int i = 0; i < this.allKingdoms.Count; i++) {
            this.allKingdoms[i].CheckForDiscoveredKingdoms();
        }
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
        if(_orderKingdomsBy == KINGDOMS_ORDERED_BY.NAME) {
            allKingdomsOrderedBy = allKingdoms.OrderByDescending(x => x.name).ToList();
        } else if (_orderKingdomsBy == KINGDOMS_ORDERED_BY.POPULATION) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.population).ToList();
        } else if (_orderKingdomsBy == KINGDOMS_ORDERED_BY.CITIES) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.cities.Count).ToList();
        } else if (_orderKingdomsBy == KINGDOMS_ORDERED_BY.EXPANSION_RATE) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.expansionRate).ToList();
        } else if (_orderKingdomsBy == KINGDOMS_ORDERED_BY.WEAPONS) {
            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.effectiveAttack).ToList();
        } 
//		else if (_orderKingdomsBy == KINGDOMS_ORDERED_BY.ARMOR) {
//            allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.effectiveDefense).ToList();
//        }
        //allKingdomsOrderedBy = allKingdoms.OrderBy(x => x.cities.Count).ToList();
        UIManager.Instance.UpdateKingdomSummary();
    }

    internal void SetOrderKingdomsBy(KINGDOMS_ORDERED_BY orderedBy) {
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
        oddActionDays[kingdom.oddActionDay].Remove(kingdom);
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
		if(krFirst.totalLike >= 1 && krSecond.totalLike >= 1){
			AlliancePool newAlliance = new AlliancePool();
			newAlliance.AddKingdomInAlliance(firstKingdom);
			newAlliance.AddKingdomInAlliance(secondKingdom);
			AddAlliancePool (newAlliance);
			Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Alliance", "create_alliance");
			newLog.AddToFillers (firstKingdom, firstKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (secondKingdom, secondKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
            newLog.AddToFillers(null, newAlliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
			newLog.AddAllInvolvedObjects (newAlliance.kingdomsInvolved.ToArray ());
			UIManager.Instance.ShowNotification (newLog);
			return true;
		}
		return false;
	}
	internal void AddAlliancePool(AlliancePool alliancePool){
		this._alliances.Add (alliancePool);
	}
	internal void RemoveAlliancePool(AlliancePool alliancePool){
		this._alliances.Remove (alliancePool);
	}
	internal void AddWarfare(Warfare warfare){
		this._kingdomWars.Add (warfare);
        this._allWarsThatOccured.Add(warfare);
    }
	internal void RemoveWarfare(Warfare warfare){
		this._kingdomWars.Remove (warfare);
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
		UpdateKingdomRankings ();
	}
	internal void RemoveKingdom(Kingdom kingdom){
		this.allKingdoms.Remove (kingdom);
//		this.kingdomRankings.Remove (kingdom.id);
		this.kingdomRankings.Remove (kingdom);
		UpdateKingdomRankings ();
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
}
