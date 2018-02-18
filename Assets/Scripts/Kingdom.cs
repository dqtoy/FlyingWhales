using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class Kingdom{
    [Space(10)]
    [Header("General Info")]
    public int id;
	public string name;
    public string kingdomTag; //Used for pathfinding
    public int kingdomTagIndex; //Used for pathfinding
    private Sprite _emblem;
    private Sprite _emblemBG;
    public RACE race;
    public int age;
	private float _population;
    private int _populationCapacity;
    private int foundationYear;
    private int foundationMonth;
    private int foundationDay;

	private KingdomTypeData _kingdomTypeData;
    private KINGDOM_SIZE _kingdomSize;
	private Kingdom _sourceKingdom;
	private Kingdom _mainThreat;
	private int _evenActionDay;

    //Resources
    //private Dictionary<RESOURCE, int> _availableResources; //only includes resources that the kingdom has bought via tile purchasing
    internal BASE_RESOURCE_TYPE basicResource;

    //Trading
    [NonSerialized] private List<Kingdom> _kingdomsInTradeDealWith;

    //Weighted Actions
    private Dictionary<Kingdom, List<WEIGHTED_ACTION>> _recentlyRejectedOffers;
    [NonSerialized] private List<Kingdom> _recentlyBrokenAlliancesWith;

    //  private int _baseArmor;
    private int _baseWeapons;
    private int _baseStability;
    private int _stability;
    private List<City> _cities;
    private List<Region> _regions;
	internal City capitalCity;
	internal Citizen king;
    internal Citizen nextInLine;
	internal List<Citizen> successionLine;
    private Dictionary<City, List<Citizen>> _citizens;

	internal Dictionary<Kingdom, KingdomRelationship> relationships;

	internal Color kingdomColor;
	internal List<History> kingdomHistory;

	[NonSerialized] private List<Kingdom> _discoveredKingdoms;

	//Plague
	internal Plague plague;

	//Daily Cumulative
	private EventRate[] _dailyCumulativeEventRate;
	
	//Tech
	private int _techLevel;
	private int _techCapacity;
	private int _techCounter;

	//The First and The Keystone
    private bool _isGrowthEnabled;

	//Serum of Alacrity
	private int _serumsOfAlacrity;

    //FogOfWar
    private FOG_OF_WAR_STATE[,] _fogOfWar;
    private Dictionary<HexTile, FOG_OF_WAR_STATE> _outerFogOfWar;
	private Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>> _fogOfWarDict;
    private Dictionary<Region, FOG_OF_WAR_STATE> _regionFogOfWarDict;

    //Crimes
    private CrimeData _crimeData;
	private CrimeDate _crimeDate;

	//Events of Kingdom
	private List<GameEvent> _activeEvents;
	private List<GameEvent> _doneEvents;

    //Expansion
    private int _currentExpansionRate;
    private int _kingdomExpansionRate;

	//Balance of Power
	private bool _isMobilizing;
    [NonSerialized] private List<Kingdom> _adjacentKingdoms;

	//Resources
	private int _foodCityCapacity;
	private int _materialCityCapacityForHumans;
	private int _materialCityCapacityForElves;
	private int _oreCityCapacity;

	private bool _isDead;
	private bool _hasBioWeapon;
	private bool _isLockedDown;
	private bool _isTechProducing;
	private bool _isMilitarize;
	private bool _isFortifying;

	private int borderConflictLoyaltyExpiration;
	private float _techProductionPercentage;
	private float _productionGrowthPercentage;

	private bool _hasUpheldHiddenHistoryBook;

	private bool _hasSecession;

	private List<Citizen> orderedMaleRoyalties;
	private List<Citizen> orderedFemaleRoyalties;
	private List<Citizen> orderedBrotherRoyalties;
	private List<Citizen> orderedSisterRoyalties;

	//Kingdom Threat
	private int _warmongerValue;

	//Alliance
	private AlliancePool _alliancePool;

	//Warfare
	private Dictionary<int, WarfareInfo> _warfareInfo;

    private float _researchRateFromKing;
    private float _draftRateFromKing;
    private float _productionRateFromKing;

    private int _stabilityDecreaseFromInvasionCounter;
    internal List<GameDate> datesStabilityDecreaseWillExpire = new List<GameDate>(); //TODO Remove this when testing is done

	internal Kingdom highestThreatAdjacentKingdomAbove50;
	internal float highestThreatAdjacentKingdomAbove50Value;

	internal Kingdom highestRelativeStrengthAdjacentKingdom;
	internal int highestRelativeStrengthAdjacentKingdomValue;

	internal bool has100OrAboveThreat;

	internal HashSet<int> checkedWarfareID;

	internal MilitaryManager2 militaryManager;

    #region getters/setters
    internal Sprite emblem {
        get { return _emblem; }
    }
    internal Sprite emblemBG {
        get { return _emblemBG; }
    }
    internal KINGDOM_TYPE kingdomType {
		get { 
			if (this._kingdomTypeData == null) {
				return KINGDOM_TYPE.NONE;
			}
			return this._kingdomTypeData.kingdomType; 
		}
	}
    internal KingdomTypeData kingdomTypeData {
		get { return this._kingdomTypeData; }
	}
    internal KINGDOM_SIZE kingdomSize {
        get { return _kingdomSize; }
    }
    internal Kingdom sourceKingdom {
		get { return this._sourceKingdom; }
	}
    internal Kingdom mainThreat {
		get { return this._mainThreat; }
	}
    internal int population {
		get { return this.cities.Sum(x => x.population); }
    }
    internal int populationCapacity {
//		get { return this._populationCapacity; }
		get { return this.cities.Sum(x => x.populationCapacity); }
    }
 //   internal Dictionary<RESOURCE, int> availableResources{
	//	get{ return this._availableResources; }
	//}
    //internal Dictionary<Kingdom, EMBARGO_REASON> embargoList {
    //    get { return this._embargoList;  }
    //}
    internal bool isDead{
		get{ return this._isDead; }
	}
    internal List<City> cities{
		get{ return this._cities; }
	}
    internal List<Region> regions {
        get { return _regions; }
    }
    internal int stability {
        get { return this._stability; }
    }
    //internal int basicResourceCount {
    //    get { return this._availableResources.Where(x => Utilities.GetBaseResourceType(x.Key) == this.basicResource).Sum(x => x.Value); }
    //}
    internal List<Kingdom> discoveredKingdoms {
        get { return this._discoveredKingdoms; }
    }
    internal int techLevel{
		get{return this._techLevel;}
	}
    internal int techCapacity{
		get{return this._techCapacity;}
	}
    internal int techCounter{
		get{return this._techCounter;}
	}
    internal int currentExpansionRate {
        get { return _currentExpansionRate; }
    }
    internal bool hasBioWeapon {
		get { return this._hasBioWeapon; }
	}
    internal EventRate[] dailyCumulativeEventRate {
		get { return this._dailyCumulativeEventRate; }
	}
    internal bool isGrowthEnabled {
        get { return _isGrowthEnabled; }
    }
    internal int serumsOfAlacrity {
		get { return this._serumsOfAlacrity; }
	}
    internal FOG_OF_WAR_STATE[,] fogOfWar {
        get { return _fogOfWar; }
    }
    internal Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>> fogOfWarDict {
        get { return _fogOfWarDict; }
    }
    internal Dictionary<Region, FOG_OF_WAR_STATE> regionFogOfWarDict {
        get { return _regionFogOfWarDict; }
    }
    internal bool isLockedDown{
		get { return this._isLockedDown;}
	}
    internal bool isTechProducing{
		get { return this._isTechProducing;}
	}
    internal bool isMilitarize{
		get { return this._isMilitarize;}
	}
    internal bool isFortifying{
		get { return this._isFortifying;}
	}
    internal float productionGrowthPercentage {
		get { return this._productionGrowthPercentage; }
	}
    internal bool hasUpheldHiddenHistoryBook{
		get { return this._hasUpheldHiddenHistoryBook;}
	}
    internal bool hasSecession{
		get { return this._hasSecession;}
	}
    internal List<GameEvent> activeEvents{
		get { return this._activeEvents;}
	}
    internal List<GameEvent> doneEvents{
		get { return this._doneEvents;}
	}
    internal int baseWeapons{
		get { return _baseWeapons;}
	}
    internal List<Kingdom> adjacentKingdoms{
        get { return _adjacentKingdoms; }
    }
    internal bool isMobilizing{
		get { return this._isMobilizing;}
	}
    internal float techProductionPercentage{
		get { return this._techProductionPercentage;}
	}
    internal int actionDay{
		get { return this._evenActionDay;}
	}
    internal int warmongerValue{
		get { return this._warmongerValue;}
	}
    internal AlliancePool alliancePool{
		get { return this._alliancePool;}
	}
    internal Dictionary<int, WarfareInfo> warfareInfo{
		get { return this._warfareInfo;}
	}
    internal int scientists {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * researchRate)); }
    }
    internal int soldiers {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * draftRate)); }
    }
    internal int workers {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * productionRate)); }
    }
	internal int soldiersCap {
		get { return Mathf.FloorToInt(population * draftRate); }
	}
    internal float draftRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.draftRate + _draftRateFromKing;
        }
    }
    internal float researchRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.researchRate + _researchRateFromKing;
        }
    }
    internal float productionRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.productionRate + _productionRateFromKing;
        }
    }
	internal int effectiveAttack{
		get{
			if (this.race != RACE.UNDEAD) {
				float mySoldiers = (float)this.soldiers;
				float numerator = 2f * mySoldiers * (float)this._baseWeapons;
				return (int)(numerator / (mySoldiers + (float)this._baseWeapons));
			} else {
				return this.population;
			}
		}
	}
    internal Dictionary<City, List<Citizen>> citizens {
        get { return _citizens; }
    }
    internal int stabilityDecreaseFromInvasionCounter {
        get { return _stabilityDecreaseFromInvasionCounter; }
    }
	internal int allianceValue {
		get { 
			if (this.alliancePool == null) {
				return 0;
			} else {
				return 1;
			}
		}
	}
    internal List<Kingdom> kingdomsInTradeDealWith {
        get { return _kingdomsInTradeDealWith; }
    }

    internal int foodCityCapacity {
		get { return this._foodCityCapacity; }
	}
	internal int materialCityCapacityForHumans {
		get { return this._materialCityCapacityForHumans; }
	}
	internal int materialCityCapacityForElves {
		get { return this._materialCityCapacityForElves; }
	}
	internal int oreCityCapacity {
		get { return this._oreCityCapacity; }
	}
    internal Dictionary<Kingdom, List<WEIGHTED_ACTION>> recentlyRejectedOffers {
        get { return _recentlyRejectedOffers; }
    }
    internal List<Kingdom> recentlyBrokenAlliancesWith {
        get { return _recentlyBrokenAlliancesWith; }
    }
    #endregion

    // Kingdom constructor paramters
    //	race - the race of this kingdom
    //	cities - the cities that this kingdom will initially own
    //	sourceKingdom (optional) - the kingdom from which this new kingdom came from
    public Kingdom(RACE race, List<HexTile> cities, Kingdom sourceKingdom = null) {
		this.id = Utilities.SetID(this);
		this.race = race;
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
        this._emblem = KingdomManager.Instance.GenerateKingdomEmblem(this);
        this._emblemBG = KingdomManager.Instance.GenerateKingdomEmblemBG();
        this.kingdomTag = name + "_" + id;
        this.kingdomTagIndex = PathfindingManager.Instance.AddNewTag(kingdomTag);
        this.king = null;
        this.nextInLine = null;
        this._kingdomSize = KINGDOM_SIZE.SMALL;
        this._mainThreat = null;
        this.successionLine = new List<Citizen>();
		this._cities = new List<City> ();
        this._regions = new List<Region>();
        this._citizens = new Dictionary<City, List<Citizen>>();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Utilities.GetColorForKingdom();
		//this._availableResources = new Dictionary<RESOURCE, int> ();
		this.relationships = new Dictionary<Kingdom, KingdomRelationship>();
		this._isDead = false;
		this._isLockedDown = false;
		this._isMilitarize = false;
		this._isFortifying = false;
		this._hasUpheldHiddenHistoryBook = false;
        //this._embargoList = new Dictionary<Kingdom, EMBARGO_REASON>();
        this._stability = 0;
		this._sourceKingdom = sourceKingdom;
		this.borderConflictLoyaltyExpiration = 0;
		this._discoveredKingdoms = new List<Kingdom>();
		this._techLevel = 0;
		this._techCounter = 0;
		this._hasBioWeapon = false;
		this.plague = null;
        this.age = 0;
        this.foundationYear = GameManager.Instance.year;
        this.foundationDay = GameManager.Instance.days;
        this.foundationMonth = GameManager.Instance.month;

        //Trading
        this._kingdomsInTradeDealWith = new List<Kingdom>();

        //Weighted Actions
        this._recentlyRejectedOffers = new Dictionary<Kingdom, List<WEIGHTED_ACTION>>();
        this._recentlyBrokenAlliancesWith = new List<Kingdom>();

        //Expansion
        this._kingdomExpansionRate = UnityEngine.Random.Range(1, 5);

        //Fog Of War
        this._fogOfWar = new FOG_OF_WAR_STATE[(int)GridMap.Instance.width, (int)GridMap.Instance.height];
        this._fogOfWarDict = new Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>>();
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.HIDDEN, new HashSet<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>())));
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.SEEN, new HashSet<HexTile>());
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.VISIBLE, new HashSet<HexTile>());
        this._regionFogOfWarDict = new Dictionary<Region, FOG_OF_WAR_STATE>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            _regionFogOfWarDict.Add(GridMap.Instance.allRegions[i], FOG_OF_WAR_STATE.HIDDEN);
        }
        this._outerFogOfWar = new Dictionary<HexTile, FOG_OF_WAR_STATE>();
        for (int i = 0; i < GridMap.Instance.outerGridList.Count; i++) {
            _outerFogOfWar.Add(GridMap.Instance.outerGridList[i], FOG_OF_WAR_STATE.HIDDEN);
            this._fogOfWarDict[FOG_OF_WAR_STATE.HIDDEN].Add(GridMap.Instance.outerGridList[i]);
        }

        this._activeEvents = new List<GameEvent> ();
		this._doneEvents = new List<GameEvent> ();
		this.orderedMaleRoyalties = new List<Citizen> ();
		this.orderedFemaleRoyalties = new List<Citizen> ();
		this.orderedBrotherRoyalties = new List<Citizen> ();
		this.orderedSisterRoyalties = new List<Citizen> ();
		this._adjacentKingdoms = new List<Kingdom> ();

		this._evenActionDay = 0;
		this._alliancePool = null;
		this._warfareInfo = new Dictionary<int, WarfareInfo>();
        this._stabilityDecreaseFromInvasionCounter = 0;
		this.highestThreatAdjacentKingdomAbove50 = null;

		this.checkedWarfareID = new HashSet<int> ();

		SetGrowthState(true);
		this.SetWarmongerValue (25);
		this.SetProductionGrowthPercentage(1f);

		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());

		if(this.race != RACE.UNDEAD){
//			AdjustPopulation(50);
			AdjustStability(50);
			AdjustBaseWeapons(25);
//			AdjustBaseArmors(25);
			//this.GenerateKingdomCharacterValues();
			this.SetLockDown(false);
			this.SetTechProduction(true);
			this.SetTechProductionPercentage(1f);
			this.UpdateTechCapacity ();
			this.SetSecession (false);
			// this.NewRandomCrimeDate (true);
			// Determine what type of Kingdom this will be upon initialization.
			this._kingdomTypeData = null;
			SetKingdomType(StoryTellingManager.Instance.GetRandomKingdomTypeForKingdom());

			this.basicResource = Utilities.GetBasicResourceForRace(race);

			Messenger.AddListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
            Messenger.AddListener("OnDayEnd", AttemptToExpand);
            Messenger.AddListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

            //SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseExpansionRatePerMonth());
            SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseBOPAttributesPerMonth());
            SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseWarWearinessPerMonth());
            SchedulingManager.Instance.AddEntry (1, 1, GameManager.Instance.year + 1, () => WarmongerDecreasePerYear ());
			//ScheduleOddDayActions();
			ScheduleActionDay();
		}
        this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

    #region Initialization Functions
    internal void CreateInitialCities(List<HexTile> initialCityLocations) {
        if (initialCityLocations.Count > 0) {
            for (int i = 0; i < initialCityLocations.Count; i++) {
                HexTile initialCityLocation = initialCityLocations[i];
                City newCity = this.CreateNewCityOnTileForKingdom(initialCityLocation, true);
                initialCityLocation.region.SetOccupant(newCity);
				initialCityLocation.emptyCityGO.SetActive (false);
            }
        }
    }
    internal void CreateInitialFamilies() {
        CreateInitialRoyalFamily();
        CreateNewChancellorFamily();
        CreateNewMarshalFamily();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            CreateNewGovernorFamily(currCity);
        }
    }
    internal void CreateInitialRoyalFamily() {
        successionLine.Clear();
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen king = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthKing = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        king.AssignBirthday(monthKing, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));

        father.isDirectDescendant = true;
        mother.isDirectDescendant = true;
        father.isDead = true;
        mother.isDead = true;

        father.AddChild(king);
        mother.AddChild(king);
        king.AddParents(father, mother);

        MarriageManager.Instance.Marry(father, mother);

        AddCitizenToKingdom(king, capitalCity);
        king.isDirectDescendant = true;
        AssignNewKing(king);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, king.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, king.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, capitalCity);
            AddCitizenToKingdom(sibling2, capitalCity);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, king.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, capitalCity);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(king);
            AddCitizenToKingdom(spouse, capitalCity);
            spouse.UpdateKingOpinion();
        }
    }
    internal void CreateNewChancellorFamily() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen chancellor = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChancellor = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        chancellor.AssignBirthday(monthChancellor, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthChancellor] + 1), (GameManager.Instance.year - chancellor.age));

        father.isDead = true;
        mother.isDead = true;

        father.AddChild(chancellor);
        mother.AddChild(chancellor);
        chancellor.AddParents(father, mother);
        AddCitizenToKingdom(chancellor, capitalCity);
        chancellor.UpdateKingOpinion();

        MarriageManager.Instance.Marry(father, mother);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, chancellor.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, chancellor.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, capitalCity);
            AddCitizenToKingdom(sibling2, capitalCity);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, chancellor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, capitalCity);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(chancellor);
            AddCitizenToKingdom(spouse, capitalCity);
            spouse.UpdateKingOpinion();
        }
        chancellor.AssignRole(ROLE.GRAND_CHANCELLOR);
    }
    internal void CreateNewMarshalFamily() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen marshal = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMarshal = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        marshal.AssignBirthday(monthMarshal, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMarshal] + 1), (GameManager.Instance.year - marshal.age));

        father.isDead = true;
        mother.isDead = true;

        father.AddChild(marshal);
        mother.AddChild(marshal);
        marshal.AddParents(father, mother);
        AddCitizenToKingdom(marshal, capitalCity);
        marshal.UpdateKingOpinion();

        MarriageManager.Instance.Marry(father, mother);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, marshal.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, marshal.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, capitalCity);
            AddCitizenToKingdom(sibling2, capitalCity);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, marshal.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, capitalCity);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(marshal);
            AddCitizenToKingdom(spouse, capitalCity);
            spouse.UpdateKingOpinion();
        }
        marshal.AssignRole(ROLE.GRAND_MARSHAL);
    }
    internal void CreateNewGovernorFamily(City cityOfFamily) {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen governor = new Citizen(cityOfFamily, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(cityOfFamily, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(cityOfFamily, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        governor.AssignRole(ROLE.GOVERNOR);

        father.AddChild(governor);
        mother.AddChild(governor);
        governor.AddParents(father, mother);
        AddCitizenToKingdom(governor, cityOfFamily);
        governor.UpdateKingOpinion();

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthGovernor = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        governor.AssignBirthday(monthGovernor, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthGovernor] + 1), (GameManager.Instance.year - governor.age));

        father.isDead = true;
        mother.isDead = true;
        MarriageManager.Instance.Marry(father, mother);

        cityOfFamily.governor = governor;

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, cityOfFamily);
            AddCitizenToKingdom(sibling2, cityOfFamily);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, cityOfFamily);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(governor);
            AddCitizenToKingdom(spouse, cityOfFamily);
            spouse.UpdateKingOpinion();
        }
    }
    internal void SetKingdomType(KINGDOM_TYPE kingdomType) {
        KINGDOM_TYPE prevKingdomType = this.kingdomType;
        switch (kingdomType) {
            case KINGDOM_TYPE.DEFENSIVE_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeNoble;
                break;
            case KINGDOM_TYPE.OFFENSIVE_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeEvil;
                break;
            case KINGDOM_TYPE.SCIENTIFIC_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeMerchant;
                break;
            case KINGDOM_TYPE.BALANCED_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeChaotic;
                break;
        }

        if (this.kingdomTypeData.dailyCumulativeEventRate != null) {
            this._dailyCumulativeEventRate = this._kingdomTypeData.dailyCumulativeEventRate;
        }

        // If the Kingdom Type Data changed
        if (prevKingdomType != this.kingdomType) {

            //Update Relationship Opinion
            UpdateAllRelationshipsLikenessFromOthers();
        }
    }
    #endregion

    #region Kingdom Death
    // Function to call if you want to determine whether the Kingdom is still alive or dead
    // At the moment, a Kingdom is considered dead if it doesnt have any cities.
    public bool isAlive() {
        if (this.cities.Count > 0) {
            return true;
        }
        return false;
    }
    /*
     * <summary>
	 * Every time a city of this kingdom dies, check if
	 * this kingdom has no more cities, if so, the kingdom is
	 * considered dead. Remove all ties from other kingdoms.
     * </summary>
	 * */
    internal void CheckIfKingdomIsDead() {
        if (this.cities.Count <= 0) {
            //Kingdom is dead
            this.DestroyKingdom();
        }
    }
    /*
     * <summary>
	 * Kill this kingdom. This removes all ties with other kingdoms.
	 * Only call this when a kingdom has no more cities.
     * </summary>
	 * */
    internal void DestroyKingdom() {
        _isDead = true;
        CancelEventKingdomIsInvolvedIn(EVENT_TYPES.ALL);
		if(this.alliancePool != null){
			LeaveAlliance (true);
		}
		this.militaryManager.DestroyAllGenerals ();
        //PathfindingManager.Instance.RemoveTag(kingdomTag);
        KingdomManager.Instance.UnregisterKingdomFromActionDays(this);
        KingdomManager.Instance.RemoveEmblemAsUsed(_emblem);
        ResolveWars();
        Messenger.RemoveListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
        Messenger.RemoveListener("OnDayEnd", AttemptToExpand);
        Messenger.RemoveListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

        Messenger.Broadcast<Kingdom>("OnKingdomDied", this);

        this.DeleteRelationships();
        KingdomManager.Instance.RemoveKingdom(this);
        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "obliterated");
        string yearsLasted = string.Empty;
        if (age == 1) {
            yearsLasted = "a year";
        } else if (age <= 0) {
            yearsLasted = "less than a year";
        } else {
            yearsLasted = age.ToString() + " years";
        }
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(null, yearsLasted, LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);

        UIManager.Instance.CheckIfShowingKingdomIsAlive(this);

        Debug.Log(this.id + " - Kingdom: " + this.name + " has died!");
        Debug.Log("Stack Trace: " + System.Environment.StackTrace);
    }
    private void CancelEventKingdomIsInvolvedIn(EVENT_TYPES eventType) {
		List<GameEvent> eventsToCancel = new List<GameEvent>(this.activeEvents);
		if (eventType == EVENT_TYPES.ALL) {
			for (int i = 0; i < eventsToCancel.Count; i++) {
				eventsToCancel [i].CancelEvent ();
			}
		}else{
			for (int i = 0; i < eventsToCancel.Count; i++) {
				if (eventsToCancel[i].eventType == eventType) {
					eventsToCancel[i].CancelEvent ();
				}
			}
		}
    }
    private void ResolveWars() {
//        List<War> warsToResolve = relationships.Values.Where(x => x.war != null).Select(x => x.war).ToList();
//        for (int i = 0; i < warsToResolve.Count; i++) {
//            warsToResolve[i].DoneEvent();
//        }

		foreach (KingdomRelationship rel in relationships.Values) {
			if (rel.sharedRelationship.isAtWar) {
				rel.ChangeWarStatus (false, null);
			}
		}
		List<WarfareInfo> warfareInfos = this._warfareInfo.Values.ToList ();
		for (int i = 0; i < warfareInfos.Count; i++) {
			warfareInfos [i].warfare.UnjoinWar (this);
		}
    }
    protected void OtherKingdomDiedActions(Kingdom kingdomThatDied) {
        if (kingdomThatDied.id != this.id) {
            RemoveRelationshipWithKingdom(kingdomThatDied);
            RemoveKingdomFromDiscoveredKingdoms(kingdomThatDied);
            //RemoveKingdomFromEmbargoList(kingdomThatDied);
            RemoveTradeDealWith(kingdomThatDied);
            RemoveAdjacentKingdom(kingdomThatDied);
            if (_mainThreat != null && _mainThreat == kingdomThatDied) {
                _mainThreat = null;
            }
        }
    }
    #endregion

    #region Relationship Functions
    internal void CreateInitialRelationships() {
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            if (KingdomManager.Instance.allKingdoms[i].id != this.id) {
                Kingdom currOtherKingdom = KingdomManager.Instance.allKingdoms[i];
                //CreateNewRelationshipWithKingdom(currOtherKingdom);
                this.relationships.Add(currOtherKingdom, new KingdomRelationship(this, currOtherKingdom));
            }
        }
    }
    /*
     * <summary>
	 * Used to create a new KingdomRelationship with the
	 * newly created kingdom. Function is listening to onCreateNewKingdom Event.
     * </summary>
	 * */
    protected void CreateNewRelationshipWithKingdom(Kingdom createdKingdom) {
        if (createdKingdom.id == this.id) {
            return;
        }

        if (relationships.ContainsKey(createdKingdom)) {
            return;
        }

        //Debug.Log(this.name + " created new relationship with " + createdKingdom.name);

        KingdomRelationship newRel1 = new KingdomRelationship(this, createdKingdom);
		relationships.Add(createdKingdom, newRel1);
		KingdomRelationship newRel2 = new KingdomRelationship(createdKingdom, this);
		createdKingdom.relationships.Add(this, newRel2);

		SharedKingdomRelationship sharedRelationship = new SharedKingdomRelationship (newRel1, newRel2);
		newRel1.sharedRelationship = sharedRelationship;
		newRel2.sharedRelationship = sharedRelationship;

		newRel1.UpdateLikeness();
		newRel2.UpdateLikeness();
    }
    /* 
     * <summary>
     * Clear all relationships from self.
     * </summary>
     * */
    protected void DeleteRelationships() {
        this.relationships.Clear();
    }
    /*
     * <summary>
     * Remove a kingdom from this kingdom's list of relationships
     * </summary>
     * */
    protected void RemoveRelationshipWithKingdom(Kingdom kingdomThatDied) {
        if (!relationships.Remove(kingdomThatDied)) {
            string errorMsg = this.name + " cannot remove relationship with " + kingdomThatDied.name + " because it only has relationships with ";
            for (int i = 0; i < relationships.Keys.Count; i++) {
                errorMsg += "\n" + relationships.Keys.ElementAt(i).id + " - " + relationships.Keys.ElementAt(i).name;
            }
            throw new Exception(errorMsg);
        }
    }
    /*
     * <summary>
     * Actions to perform when a relationship that this kingdom owns, deteriorates
     * </summary>
     * */
    internal void OnRelationshipDeteriorated(KingdomRelationship relationship, GameEvent gameEventTrigger, bool isDiscovery, ASSASSINATION_TRIGGER_REASONS assassinationReasons) {
//        if (assassinationReasons != ASSASSINATION_TRIGGER_REASONS.NONE) {
//            TriggerAssassination(relationship, gameEventTrigger, assassinationReasons);
//        }
    }
    /*
    * <summary>
    * Actions to perform when a relationship that this kingdom owns, improves
    * </summary>
    * */
    internal void OnRelationshipImproved(KingdomRelationship relationship) {
        //Improvement of Relationship
//        int chance = UnityEngine.Random.Range(0, 100);
//        int value = 0;
//        if (relationship.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//            value = 5;
//        } else if (relationship.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//            value = 15;
//        } else {
//            value = 25;
//        }
//        if (chance < value) {
//            CancelInvasionPlan(relationship);
//        }
    }
    /*
     * <summary>
     * Get all kingdoms that this kingdom has a specific relationshipStatus
     * </summary>
     * <param name="relationshipStatuses">Relationship Statuses to be checked</param>
     * <param name="discoveredOnly">Should only return discovered kingdoms?</param>
     * */
	internal List<Kingdom> GetKingdomsByRelationship(KINGDOM_RELATIONSHIP_STATUS[] relationshipStatuses, Kingdom exception = null, bool discoveredOnly = true) {
        List<Kingdom> kingdomsWithRelationshipStatus = new List<Kingdom>();
		if(discoveredOnly){
			foreach (Kingdom currKingdom in relationships.Keys) {
				if(exception != null && exception.id == currKingdom.id){
					continue;
				}
				//        for (int i = 0; i < relationships.Count; i++) {
				//            Kingdom currKingdom = relationships.Keys.ElementAt(i);
				if (!discoveredKingdoms.Contains(currKingdom)) {
					continue;
				}

				KINGDOM_RELATIONSHIP_STATUS currStatus = relationships[currKingdom].relationshipStatus;
				if (relationshipStatuses.Contains(currStatus)) {
					kingdomsWithRelationshipStatus.Add(currKingdom);
				}
			}
		}else{
			foreach (Kingdom currKingdom in relationships.Keys) {
				//        for (int i = 0; i < relationships.Count; i++) {
				//            Kingdom currKingdom = relationships.Keys.ElementAt(i);
				if(exception != null && exception.id == currKingdom.id){
					continue;
				}
				KINGDOM_RELATIONSHIP_STATUS currStatus = relationships[currKingdom].relationshipStatus;
				if (relationshipStatuses.Contains(currStatus)) {
					kingdomsWithRelationshipStatus.Add(currKingdom);
				}
			}
		}
        return kingdomsWithRelationshipStatus;
    }
    internal KingdomRelationship GetRelationshipWithKingdom(Kingdom kingdom, bool ignoreException = false) {
        if (relationships.ContainsKey(kingdom)) {
            return relationships[kingdom];
        } else {
            if (ignoreException) {
                return null;
            } else {
                throw new Exception(this.name + " does not have relationship with " + kingdom.name);
            }
        }
        
    }
//    internal void UpdateMutualRelationships() {
//		foreach (KingdomRelationship currRel in relationships.Values) {
////        for (int i = 0; i < relationships.Count; i++) {
////            KingdomRelationship currRel = relationships.Values.ElementAt(i);
//            Kingdom targetKingdom = currRel.targetKingdom;
//            KingdomRelationship targetKingdomRel = targetKingdom.GetRelationshipWithKingdom(this);

//            if (targetKingdomRel == null || currRel == null) {
//                return;
//            }

//            currRel.ResetMutualRelationshipModifier();
//            targetKingdomRel.ResetMutualRelationshipModifier();

//			List<Kingdom> sourceKingRelationships = GetKingdomsByRelationship (new
//           [] { RELATIONSHIP_STATUS.HATE, RELATIONSHIP_STATUS.SPITE,
//				RELATIONSHIP_STATUS.AFFECTIONATE, RELATIONSHIP_STATUS.LOVE
//			}, targetKingdom);

//			List<Kingdom> targetKingRelationships = targetKingdom.GetKingdomsByRelationship (new
//                [] { RELATIONSHIP_STATUS.HATE, RELATIONSHIP_STATUS.SPITE,
//				RELATIONSHIP_STATUS.AFFECTIONATE, RELATIONSHIP_STATUS.LOVE
//			}, this);

////            List<Kingdom> kingdomsInCommon = sourceKingRelationships.Intersect(targetKingRelationships).ToList();
//			foreach (var currKingdom in sourceKingRelationships.Intersect(targetKingRelationships)) {
////            for (int j = 0; j < kingdomsInCommon.Count; j++) {
////                Kingdom currKingdom = kingdomsInCommon[j];
//                KingdomRelationship relSourceKingdom = this.GetRelationshipWithKingdom(currKingdom);
//                KingdomRelationship relTargetKingdom = targetKingdom.GetRelationshipWithKingdom(currKingdom);

//                if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE ||
//                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                        targetKingdomRel.AddMutualRelationshipModifier(10);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE ||
//                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                        currRel.AddMutualRelationshipModifier(10);
//                        targetKingdomRel.AddMutualRelationshipModifier(10);
//                    }
//                }
//            }
//        }
//    }
    internal void ResetRelationshipModifiers() {
        for (int i = 0; i < relationships.Count; i++) {
            KingdomRelationship currRel = relationships.ElementAt(i).Value;
            //currRel.ResetMutualRelationshipModifier();
            currRel.ResetEventModifiers();
        }
    }
    internal void UpdateAllRelationshipsLikeness() {
        if (this.king != null) {
			foreach (KingdomRelationship relationship in relationships.Values) {
                //if (relationship.isDiscovered) {
                    relationship.UpdateLikeness();
                //}
			}
        }
    }
	internal void UpdateAllRelationshipsLikenessFromOthers() {
		if (this.king != null) {
			foreach (KingdomRelationship relationship in relationships.Values) {
				KingdomRelationship relationshipFromOther = relationship.targetKingdom.GetRelationshipWithKingdom(this);
				relationshipFromOther.UpdateLikeness ();
			}
		}
	}
    #endregion

	/*
	 * This function is listening to the onWeekEnd Event. Put functions that you want to
	 * happen every tick here.
	 * */
	//protected void KingdomTickActions(){
 //       //if (_isGrowthEnabled) {
 //       //    this.AttemptToExpand();
 //       //}
	//	this.IncreaseTechCounterPerTick();
 //       //this.TriggerEvents();
 //   }
//    private void AdaptToKingValues() {
//		if(!this.isDead){
//			for (int i = 0; i < _dictCharacterValues.Count; i++) {
//				CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
//				if (king.importantCharacterValues.ContainsKey(currValue)) {
//					UpdateSpecificCharacterValue(currValue, 1);
//				} else {
//					UpdateSpecificCharacterValue(currValue, -1);
//				}
//			}
//			UpdateKingdomCharacterValues();
//			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => AdaptToKingValues());
//		}
////        if(GameManager.Instance.days == 1 && GameManager.Instance.month == 1) {
////            for (int i = 0; i < _dictCharacterValues.Count; i++) {
////                CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
////                if (king.importantCharacterValues.ContainsKey(currValue)) {
////                    UpdateSpecificCharacterValue(currValue, 1);
////                } else {
////                    UpdateSpecificCharacterValue(currValue, -1);
////                }
////            }
////            UpdateKingdomCharacterValues();
////        }
//    }
    private void AttemptToAge() {
		if(!this.isDead){
			age += 1;
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
		}

//        if(GameManager.Instance.year > foundationYear && GameManager.Instance.month == foundationMonth && GameManager.Instance.days == foundationDay) {
//            age += 1;
//        }
    }
	private void ScheduleEvents(){
//		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerSlavesMerchant());
		//SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerHypnotism());
		//SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerKingsCouncil());

		int month = UnityEngine.Random.Range (1, 5);
		SchedulingManager.Instance.AddEntry (month, UnityEngine.Random.Range(1, GameManager.daysInMonth[month]), GameManager.Instance.year, () => TriggerCrime());
	}

    #region Expansion Functions
    /*
	 * Kingdom will attempt to expand. 
	 * Expansion Rate is added every day. Value must reach 
     * KingdomManager.KINGDOM_MAX_EXPANSION_RATE to create a Settler.
	 * */
    HexTile expandableTile = null;
    private void AttemptToExpand() {
        if (HasActiveEvent(EVENT_TYPES.EXPANSION)) {
            //Kingdom has a currently active expansion event
            return;
        }

//        if(expandableTile == null) {
//            expandableTile = CityGenerator.Instance.GetExpandableTileForKingdom(this);
//        } else {
//            if (expandableTile.isOccupied) {
//                expandableTile = CityGenerator.Instance.GetExpandableTileForKingdom(this);
//            }
//        }
//
//        if (expandableTile == null) {
//            if(_currentExpansionRate != 0) {
//                ResetCurrentExpansionRate();
//            }
//            return;
//        }

		if(!CityGenerator.Instance.HasStillExpandableTile(this)){
			if(_currentExpansionRate != 0) {
				ResetCurrentExpansionRate();
			}
			return;
		}

        if(_currentExpansionRate < KingdomManager.KINGDOM_MAX_EXPANSION_RATE) {
            IncreaseExpansionRate();
        }
    
        if (_currentExpansionRate == KingdomManager.KINGDOM_MAX_EXPANSION_RATE) {
            EventCreator.Instance.CreateExpansionEvent(this);
        }
    }

    /*
     * Expansion Rate Gain is based on three values:
     *  - Kingdom Expansion Rate (between 1 to 4) specified at the start of a Kingdom
     *  - King Efficiency (between -1 to 1) based on Efficiency trait
     *  - random value (between 6 - 10) randomized every day
     * */
    private void IncreaseExpansionRate() {
        int expansionRateGained = _kingdomExpansionRate;
        expansionRateGained += king.GetExpansionRateContribution();
        expansionRateGained += UnityEngine.Random.Range(6, 11);
        AdjustCurrentExpansionRate(expansionRateGained);
    }
    //private void IncreaseExpansionRatePerMonth() {
    //    //Reschedule next month
    //    GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
    //    dueDate.AddMonths(1);
    //    SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseExpansionRatePerMonth());

    //    if (CityGenerator.Instance.GetExpandableTileForKingdom(this) == null) {
    //        //set expansion rate to 0 and don't increase expansion rate until kingdom can expand
    //        ResetExpansionRate();
    //        return;
    //    }
    //    if (_expansionRate < GridMap.Instance.numOfRegions) {
    //        AdjustExpansionRate(GetMonthlyExpansionRateIncrease());
    //    }
    //}
    //internal int GetMonthlyExpansionRateIncrease() {
    //    int monthlyExpansionRate = king.GetExpansionRateContribution();
    //    for (int i = 0; i < cities.Count; i++) {
    //        monthlyExpansionRate += cities[i].governor.GetExpansionRateContribution();
    //    }
    //    return monthlyExpansionRate;
    //}
    internal void ResetCurrentExpansionRate() {
        _currentExpansionRate = 0;
        KingdomManager.Instance.UpdateKingdomList();
    }
    private void AdjustCurrentExpansionRate(int adjustment) {
        _currentExpansionRate += adjustment;
        _currentExpansionRate = Mathf.Clamp(_currentExpansionRate, 0, KingdomManager.KINGDOM_MAX_EXPANSION_RATE);
        KingdomManager.Instance.UpdateKingdomList();
    }
    #endregion

    #region Odd Day Actions
    //private void ScheduleOddDayActions() {
    //    this._oddActionDay = KingdomManager.Instance.GetOddActionDay(this);
    //    //KingdomManager.Instance.IncrementOddActionDay();
    //    SchedulingManager.Instance.AddEntry(GameManager.Instance.month, _oddActionDay, GameManager.Instance.year, () => OddDayActions());
    //}
    //private void OddDayActions() {
    //    if (_isGrowthEnabled) {
    //        AttemptToExpand();
    //    }
    //    GameDate nextActionDay = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
    //    nextActionDay.AddMonths(1);
    //    SchedulingManager.Instance.AddEntry(nextActionDay.month, nextActionDay.day, nextActionDay.year, () => OddDayActions());
    //}
    #endregion

    #region Trading
    internal void AddTradeDealWith(Kingdom otherKingdom) {
        if (!_kingdomsInTradeDealWith.Contains(otherKingdom)) {
            _kingdomsInTradeDealWith.Add(otherKingdom);
        }
    }
    internal void RemoveTradeDealWith(Kingdom otherKingdom) {
         _kingdomsInTradeDealWith.Remove(otherKingdom);
    }
    internal bool IsTradeDealStillNeeded(Kingdom otherKingdom) {
        Dictionary<RESOURCE_TYPE, int> deficitOfThisKingdom = this.GetDeficitResourcesFor(otherKingdom);
        Dictionary<RESOURCE_TYPE, int> surplusOfOtherKingdom = otherKingdom.GetSurplusResourcesFor(this);
        for (int i = 0; i < deficitOfThisKingdom.Keys.Count; i++) {
            RESOURCE_TYPE deficitResource = deficitOfThisKingdom.Keys.ElementAt(i);
            if (surplusOfOtherKingdom.ContainsKey(deficitResource)) {
                //other kingdom still has a surplus of a resource that this kingdom has a deficit in
                return true;
            }
        }
        return false;
    }
    internal void LeaveTradeDealWith(Kingdom otherKingdom) {
        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "TradeDeal", "trade_deal_leave");
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(otherKingdom, otherKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom>() { this, otherKingdom });
        RemoveTradeDealWith(otherKingdom);
        otherKingdom.RemoveTradeDealWith(this);
    }
    //internal void AddKingdomToEmbargoList(Kingdom kingdomToAdd, EMBARGO_REASON embargoReason = EMBARGO_REASON.NONE) {
    //    if (!this._embargoList.ContainsKey(kingdomToAdd)) {
    //        this._embargoList.Add(kingdomToAdd, embargoReason);
    //    }

    //}
    //internal void RemoveKingdomFromEmbargoList(Kingdom kingdomToRemove) {
    //    this._embargoList.Remove(kingdomToRemove);
    //}
    #endregion

    #region City Management
    /* 
     * <summary>
	 * Create a new city obj on the specified hextile.
	 * Then add it to this kingdoms cities.
     * </summary>
	 * */
    internal City CreateNewCityOnTileForKingdom(HexTile tile, bool hasInitialPopulation = false) {
        City createdCity = CityGenerator.Instance.CreateNewCity(tile, this);
        this.AddCityToKingdom(createdCity);
		if(hasInitialPopulation){
			createdCity.AdjustPopulation (100f);
		}
        return createdCity;
    }
    /* 
     * <summary>
     * Add a city to this kingdom.
     * Recompute kingdom type data, available resources and
     * daily growth of all cities. Assign city as capital city
     * if city is first city in kingdom.
     * </summary>
     * */
    internal void AddCityToKingdom(City city) {
        this._cities.Add(city);
        _regions.Add(city.region);
        UpdateKingdomSize();
//        UpdatePopulationCapacity();
        if ((capitalCity == null || capitalCity.isDead) && this._cities.Count == 1 && this._cities[0] != null) {
            SetCapitalCity(this._cities[0]);
        }
        if (!_citizens.ContainsKey(city)) {
            _citizens.Add(city, new List<Citizen>());
        }
        KingdomManager.Instance.UpdateKingdomList();
    }
    /* 
     * <summary>
     * Remove city from this kingdom.
     * Check if kingdom is dead beacuse of city removal.
     * If not, recompute this kingdom's capital city, kingdom type data, 
     * available resources, and daily growth of remaining cities.
     * </summary>
     * */
    internal List<Citizen> RemoveCityFromKingdom(City city) {
		if (!this._cities.Remove(city)) {
			Debug.LogError ("CITY REMOVAL ERROR Kingdom Name: " + this.name);
			Debug.LogError ("CITY REMOVAL ERROR City Name: " + city.name);
			Debug.LogError ("CITY REMOVAL ERROR City Kingdom Name: " + city.kingdom.name);
			UIManager.Instance.Pause ();
		}
        //this._cities.Remove(city);
        _regions.Remove(city.region);
        this.CheckIfKingdomIsDead();
        if (!this.isDead) {
            UpdateKingdomSize();
//            UpdatePopulationCapacity();
            RevalidateKingdomAdjacency(city);
			SetCapitalCity(this._cities[0]);
            TransferCitizensFromCityToCapital(city);
            KingdomManager.Instance.UpdateKingdomList();
        }
        List<Citizen> remainingCitizens = new List<Citizen>();
        if (_citizens.ContainsKey(city)) {
            remainingCitizens.AddRange(_citizens[city]);
        }
        _citizens.Remove(city);
        UIManager.Instance.UpdateKingdomCitiesMenu();
        return remainingCitizens;
    }
    internal void SetCapitalCity(City city) {
        this.capitalCity = city;
        HexTile habitableTile;
        if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {
            for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {
                habitableTile = CityGenerator.Instance.stoneHabitableTiles[i];
                this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(city.hexTile, habitableTile));
            }
        } else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
            for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {
                habitableTile = CityGenerator.Instance.woodHabitableTiles[i];
                this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(city.hexTile, habitableTile));
            }
        }
    }
    private void UpdateKingdomSize() {
        if(cities.Count < KingdomManager.Instance.smallToMediumReq) {
            _kingdomSize = KINGDOM_SIZE.SMALL;
        } else if (cities.Count >= KingdomManager.Instance.smallToMediumReq && cities.Count < KingdomManager.Instance.mediumToLargeReq) {
            _kingdomSize = KINGDOM_SIZE.MEDIUM;
        } else if (cities.Count >= KingdomManager.Instance.mediumToLargeReq) {
            _kingdomSize = KINGDOM_SIZE.LARGE;
        }
		if (this.militaryManager != null) {
			this.militaryManager.UpdateMaxGenerals ();
		}
    }
    #endregion

    #region Citizen Management
    internal void AddCitizenToKingdom(Citizen citizenToAdd, City cityCitizenBelongsTo) {
        citizenToAdd.city = cityCitizenBelongsTo;
        if (_citizens.ContainsKey(cityCitizenBelongsTo)) {
            if (!_citizens[cityCitizenBelongsTo].Contains(citizenToAdd)) {
                _citizens[cityCitizenBelongsTo].Add(citizenToAdd);
            }
        } else {
            _citizens.Add(cityCitizenBelongsTo, new List<Citizen>() { citizenToAdd });
        }
    }
    internal void RemoveCitizenFromKingdom(Citizen citizenToRemove, City cityCitizenBelongedTo) {
        if (_citizens.ContainsKey(cityCitizenBelongedTo)) {
            _citizens[cityCitizenBelongedTo].Remove(citizenToRemove);
        }
        //else {
        //    throw new Exception(citizenToRemove.role + " " + citizenToRemove.name + " cannot be removed because " + cityCitizenBelongedTo.name + " is not a city of " + this.name);
        //}
    }
    internal bool IsElligibleForRebellion() {
        if(stability <= -50 && kingdomSize != KINGDOM_SIZE.SMALL) {
            for (int i = 0; i < cities.Count; i++) {
                City currCity = cities[i];
                if (currCity.citizens.Where(x => x.role != ROLE.KING && x.role != ROLE.UNTRAINED && x.loyaltyToKing <= -50).Any()) {
                    return true;
                }
            }
        }
        return false;
    }
    internal List<Citizen> GetCitizensForRebellion() {
        List<Citizen> citizensForRebellion = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            citizensForRebellion.AddRange(currCity.citizens.Where(x => x.role != ROLE.KING && x.role != ROLE.UNTRAINED && x.loyaltyToKing <= -50));
        }
        return citizensForRebellion;
    }
    internal Citizen GetCitizenWithRoleInKingdom(ROLE role) {
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if(currCitizen.role == role) {
                    return currCitizen;
                }
            }
        }
        return null;
    }
    internal List<Citizen> GetCitizensWithRoleInKingdom(ROLE role) {
        List<Citizen> citizensWithRole = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if (currCitizen.role == role) {
                    if (!citizensWithRole.Contains(currCitizen)) {
                        citizensWithRole.Add(currCitizen);
                    }
                }
            }
        }
        return citizensWithRole;
    }
    internal void TransferCitizensFromCityToCapital(City sourceCity, HashSet<Citizen> exceptions = null) {
        List<Citizen> importantCitizensInCity = new List<Citizen>(sourceCity.citizens.Where(x => x.role == ROLE.KING 
            || x.role == ROLE.GRAND_CHANCELLOR || x.role == ROLE.GRAND_MARSHAL));

        //Get Families of important citizens
        List<Citizen> citizensToTransfer = new List<Citizen>();
        for (int i = 0; i < importantCitizensInCity.Count; i++) {
            Citizen currImportantCitizen = importantCitizensInCity[i];
            citizensToTransfer.Add(currImportantCitizen);
            List<Citizen> familyOfCurrCitizen = currImportantCitizen.GetRelatives(-1);
            for (int j = 0; j < familyOfCurrCitizen.Count; j++) {
                Citizen currFamilyMember = familyOfCurrCitizen[j];
                if (!citizensToTransfer.Contains(currFamilyMember) && (exceptions == null || !exceptions.Contains(currFamilyMember)) && currFamilyMember.city.kingdom.id == currImportantCitizen.city.kingdom.id) {
                    citizensToTransfer.Add(currFamilyMember);
                }
            }
        }
        for (int i = 0; i < citizensToTransfer.Count; i++) {
            Citizen currCitizen = citizensToTransfer[i];
            RemoveCitizenFromKingdom(currCitizen, currCitizen.city);
            AddCitizenToKingdom(currCitizen, capitalCity);
        }
    }
    #endregion

    #region Succession
    internal void UpdateKingSuccession() {
		orderedMaleRoyalties.Clear ();
		orderedFemaleRoyalties.Clear ();
		orderedBrotherRoyalties.Clear ();
		orderedSisterRoyalties.Clear ();

		for (int i = 0; i < this.successionLine.Count; i++) {
			if (this.successionLine [i].generation > this.king.generation) {
				if (this.successionLine [i].gender == GENDER.MALE) {
					orderedMaleRoyalties.Add (this.successionLine [i]);
				} else {
					orderedFemaleRoyalties.Add (this.successionLine [i]);
				}
			}
		}

		for (int i = 0; i < this.successionLine.Count; i++) {
			if ((this.successionLine [i].father != null && this.king.father != null) && this.successionLine [i].father.id == this.king.father.id) {
				if (this.successionLine [i].gender == GENDER.MALE) {
					orderedBrotherRoyalties.Add (this.successionLine [i]);
				}else{
					orderedSisterRoyalties.Add (this.successionLine [i]);
				}
			}
		}

        this.successionLine.Clear();
        this.successionLine.AddRange (orderedMaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));
		this.successionLine.AddRange (orderedFemaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));

		this.successionLine.AddRange (orderedBrotherRoyalties.OrderByDescending (x => x.age));
		this.successionLine.AddRange (orderedSisterRoyalties.OrderByDescending (x => x.age));

        List<Citizen> invalidCitizens = new List<Citizen>();
        //Validate Succession line
        for (int i = 0; i < successionLine.Count; i++) {
            Citizen currSuccessor = successionLine[i];
            if (currSuccessor.city.kingdom.id != king.city.kingdom.id || currSuccessor.id == this.king.id || currSuccessor.role == ROLE.KING) {
                //successor is of a different kingdom!
                invalidCitizens.Add(currSuccessor);
            }
        }

        for (int i = 0; i < invalidCitizens.Count; i++) {
            successionLine.Remove(invalidCitizens[i]);
        }

        Citizen newNextInLine = successionLine.FirstOrDefault();
        if(newNextInLine != null && (newNextInLine.role == ROLE.KING || newNextInLine.role == ROLE.QUEEN)) {
            throw new Exception("New next in line " + newNextInLine.name + " is a " + newNextInLine.role.ToString() + " which is invalid!");
        }

        if (newNextInLine != null && nextInLine != null && newNextInLine.id != nextInLine.id && nextInLine.role == ROLE.CROWN_PRINCE) {
            //next in line is no longer the next in line
            nextInLine.AssignRole(ROLE.UNTRAINED);
        }
        nextInLine = newNextInLine;
        if(newNextInLine != null && newNextInLine.role != ROLE.CROWN_PRINCE) {
            newNextInLine.AssignRole(ROLE.CROWN_PRINCE);
        }
        
    }
    internal void ChangeSuccessionLineRescursively(Citizen royalty) {
        if (this.king.id != royalty.id) {
            if (!royalty.isDead) {
                this.successionLine.Add(royalty);
            }
        }

        for (int i = 0; i < royalty.children.Count; i++) {
            if (royalty.children[i] != null) {
                this.ChangeSuccessionLineRescursively(royalty.children[i]);
            }
        }
    }
    internal void RemoveFromSuccession(Citizen citizen) {
        if (citizen != null) {
            for (int i = 0; i < this.successionLine.Count; i++) {
                if (this.successionLine[i].id == citizen.id) {
                    this.successionLine.RemoveAt(i);
                    break;
                }
            }
        }
    }
    internal Citizen CreateNewKing() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen king = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);

        MONTH monthKing = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        king.AssignBirthday(monthKing, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));
        king.city.kingdom.AddCitizenToKingdom(king, capitalCity);
        king.CreateFamily();

        return king;
    }
    internal void AssignNewKing(Citizen newKing = null) {
        if(newKing == null) { 
            //A new king was not specified, check succession lines first, if there are no successors generate a new king
            if(successionLine.Count > 0) {
                newKing = successionLine.First();
            } else {
                newKing = CreateNewKing();
                List<Citizen> familyOfPreviousKing = new List<Citizen>(this.king.GetRelatives(-1));
                familyOfPreviousKing.Add(this.king);
                //Remove family of previous king from kingdom
                for (int i = 0; i < familyOfPreviousKing.Count; i++) {
                    Citizen currFamilyMember = familyOfPreviousKing[i];
                    RemoveCitizenFromKingdom(currFamilyMember, currFamilyMember.city);
                }
            }
        }

        if(newKing == null) {
            throw new Exception("No new king was generated for " + name);
        }

        if (!newKing.isDirectDescendant) {
            Utilities.ChangeDescendantsRecursively(newKing, true);
            if (this.king != null) {
                Utilities.ChangeDescendantsRecursively(this.king, false);
            }
        }

        this.king = newKing;
        newKing.AssignRole(ROLE.KING);
        ((King)newKing.assignedRole).SetOwnedKingdom(this);

        nextInLine = null;
        this.successionLine.Clear();
        ChangeSuccessionLineRescursively(newKing);
        this.successionLine.AddRange(newKing.GetSiblings());
        UpdateKingSuccession();

        this.UpdateProductionRatesFromKing();
        this.UpdateAllRelationshipsLikeness();
        this.UpdateAllCitizensOpinionOfKing();

    }
    internal void UpdateAllCitizensOpinionOfKing() {
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if (currCitizen.id != king.id) {
                    currCitizen.UpdateKingOpinion();
                }
            }
        }
    }
    #endregion

    #region War
//    internal void AdjustExhaustionToAllRelationship(int amount) {
//        for (int i = 0; i < relationships.Count; i++) {
//            relationships.ElementAt(i).Value.AdjustExhaustion(amount);
//        }
//    }
	internal void ConquerCity(City city){
		if (this.id != city.kingdom.id) {
			if(!city.kingdom.HasStillGeneralInLocation(city.hexTile)){
				city.ConquerCity(this);
			}
		}
	}
    #endregion

    #region Kingdom Tile Management
    internal void HighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            this.cities[i].HighlightAllOwnedTiles(69f / 255f);
        }
    }
    internal void UnHighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities[i].id) {
                continue;
            }
            this.cities[i].UnHighlightAllOwnedTiles();
        }
    }
    #endregion


    internal MILITARY_STRENGTH GetMilitaryStrengthAgainst(Kingdom kingdom){
		int sourceMilStrength = this.GetAllCityHp ();
		int targetMilStrength = kingdom.GetAllCityHp ();

		int fiftyPercent = (int)(targetMilStrength * 0.50f);
		int twentyPercent = (int)(targetMilStrength * 0.20f);
//		Debug.Log ("TARGET MILITARY STRENGTH: " + targetMilStrength);
//		Debug.Log ("SOURCE MILITARY STRENGTH: " + sourceMilStrength);
		if(sourceMilStrength == 0 && targetMilStrength == 0){
			Debug.Log (this.name + "'s military is COMPARABLE to " + kingdom.name);
			return MILITARY_STRENGTH.COMPARABLE;
		}else{
			if(sourceMilStrength > (targetMilStrength + fiftyPercent)){
				Debug.Log (this.name + "'s military is MUCH STRONGER than " + kingdom.name);
				return MILITARY_STRENGTH.MUCH_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength + twentyPercent)){
				Debug.Log (this.name + "'s military is SLIGHTLY STRONGER than " + kingdom.name);
				return MILITARY_STRENGTH.SLIGHTLY_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength - twentyPercent)){
				Debug.Log (this.name + "'s military is COMPARABLE to " + kingdom.name);
				return MILITARY_STRENGTH.COMPARABLE;
			}else if(sourceMilStrength > (targetMilStrength - fiftyPercent)){
				Debug.Log (this.name + "'s military is SLIGHTLY WEAKER than " + kingdom.name);
				return MILITARY_STRENGTH.SLIGHTLY_WEAKER;
			}else{
				Debug.Log (this.name + "'s military is MUCH WEAKER than " + kingdom.name);
				return MILITARY_STRENGTH.MUCH_WEAKER;
			}

		}
	}

	internal int GetAllCityHp(){
		int total = 0;
		for(int i = 0; i < this.cities.Count; i++){
			total += this.cities[i].hp;
		}
		return total;
	}
	internal int GetWarCount(){
		int total = 0;
//		for (int i = 0; i < relationships.Count; i++) {
//			if(relationships.ElementAt(i).Value.isAtWar){
//				total += 1;
//			}
//		}
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			if(relationship.sharedRelationship.isAtWar){
				total += 1;
			}
		}
		return total;
	}
	internal City GetNearestCityFromKing(List<City> cities){
		City nearestCity = null;
		int nearestDistance = 0;
		for(int i = 0; i < cities.Count; i++){
			List<HexTile> path = PathGenerator.Instance.GetPath (cities [i].hexTile, this.king.city.hexTile, PATHFINDING_MODE.AVATAR);
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

	#region Resource Management
//    /*
//     * Gets a list of resources that otherKingdom does not have access to (By self or by trade).
//    * Will compare to this kingdoms available resources (excl. resources from trade)
//    * */
//    internal List<RESOURCE> GetResourcesOtherKingdomDoesNotHave(Kingdom otherKingdom) {
//        List<RESOURCE> resourcesOtherKingdomDoesNotHave = new List<RESOURCE>();
////        List<RESOURCE> allAvailableResourcesOfOtherKingdom = otherKingdom.availableResources.Keys.ToList();
//		bool hasContainedResource = false;
//		foreach (RESOURCE currKey in this._availableResources.Keys) {
//			hasContainedResource = false;
//			foreach (RESOURCE otherCurrKey in otherKingdom.availableResources.Keys) {
//				if(otherCurrKey == currKey){
//					hasContainedResource = true;
//					break;
//				}
//			}
//			if(!hasContainedResource){
//				resourcesOtherKingdomDoesNotHave.Add(currKey);
//			}
//		}
//        return resourcesOtherKingdomDoesNotHave;
//    }
    /*
     * <summary>
     * Set growth state of kingdom, disabling growth will prevent expansion,
     * building of new settlements and pregnancy
     * */
    internal void SetGrowthState(bool state) {
        _isGrowthEnabled = state;
    }
    #endregion

    #region Tech
    internal int GetMonthlyTechGain() {
        int monthlyTechGain = GetTechContributionFromCitizens();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead) {
                monthlyTechGain += currCity.techPoints;
            }
        }

		int scientistFactor = (int)Math.Sqrt (50 * scientists);
        //Tech Gains
		monthlyTechGain = ((2 * scientistFactor * monthlyTechGain) / (scientistFactor + monthlyTechGain));
        monthlyTechGain = Mathf.FloorToInt(monthlyTechGain * techProductionPercentage);
        return monthlyTechGain;
    }
    internal int GetTechContributionFromCitizens() {
        int techContributionsFromCitizens = 0;
        techContributionsFromCitizens += king.GetTechContribution();
        for (int i = 0; i < cities.Count; i++) {
            techContributionsFromCitizens += cities[i].governor.GetTechContribution();
        }
        return techContributionsFromCitizens;
    }
	private void UpdateTechCapacity(){
		this._techCapacity = 500 + ((800 + (500 * this._techLevel)) * this._techLevel);
	}
	internal void AdjustTechCounter(int amount){
		this._techCounter += amount;
		this._techCounter = Mathf.Clamp(this._techCounter, 0, this._techCapacity);
		if(this._techCounter == this._techCapacity){
			this.UpgradeTechLevel (1);
		}
        if(UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UIManager.Instance.UpdateTechMeter();
        }
	}
	internal void UpgradeTechLevel(int amount){
		this._techLevel += amount;
		if(this._techLevel < 0){
			amount -= this._techLevel;
			this._techLevel = 0;
		}
		this._techCounter = 0;
		//AdjustPowerPointsToAllCities(amount);
  //      AdjustDefensePointsToAllCities(amount);
		this.UpdateTechCapacity();
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UIManager.Instance.UpdateTechMeter();
        }
    }
    internal void DegradeTechLevel(int amount) {
        _techLevel -= amount;
        //if(_techLevel >= 0) {
        //    AdjustPowerPointsToAllCities(-amount);
        //    AdjustDefensePointsToAllCities(-amount);
        //}
        _techLevel = Mathf.Max(0, _techLevel);
        this._techCounter = 0;
        UpdateTechCapacity();
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UIManager.Instance.UpdateTechMeter();
        }
    }
	#endregion
	
	#region Discovery
    /*
     * Check all the neighburs of the border tiles and owned tiles of all this kingdom's
     * cities, and check if any of them are owned by another kingdom, if so,
     * the two kingdoms have now discovered each other.
     * */
    internal void CheckForDiscoveredKingdoms() {
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            List<HexTile> tilesToCheck = currCity.ownedTiles.Union(currCity.borderTiles).ToList();
            for (int j = 0; j < tilesToCheck.Count; j++) {
                //Get all neighbour tiles that are owned, but not by this kingdom, 
                //and that kingdom is not already added to this kingdom's discovered kingdoms.
                List<HexTile> neighbours = tilesToCheck[j].AllNeighbours;
                    //.Where(x => x.ownedByCity != null && x.ownedByCity.kingdom.id != this.id && !this._discoveredKingdoms.Contains(x.ownedByCity.kingdom))
                    //.ToList();
                for (int k = 0; k < neighbours.Count; k++) {
                    HexTile currNeighbour = neighbours[k];
                    if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                        && currNeighbour.ownedByCity.kingdom.id != this.id) {
                        Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;
						KingdomManager.Instance.DiscoverKingdom (this, otherKingdom);
                    } else if (currNeighbour.isBorder) {
                        for (int l = 0; l < currNeighbour.isBorderOfCities.Count; l++) {
                            Kingdom otherKingdom = currNeighbour.isBorderOfCities[l].kingdom;
                            if (otherKingdom.id != this.id && !this.discoveredKingdoms.Contains(otherKingdom)) {
                                KingdomManager.Instance.DiscoverKingdom(this, otherKingdom);
                            }
                        }
                    }
                }
            }
        }
    }
    /*
     * Check all the neighbours of a HexTile and check if any of them are owned by another kingdom, if so,
     * the two kingdoms have now discovered each other.
     * */
    internal void CheckForDiscoveredKingdoms(City city) {
        //Get all neighbour tiles that are owned, but not by this kingdom, 
        //and that kingdom is not already added to this kingdom's discovered kingdoms.
        List<HexTile> tilesToCheck = city.ownedTiles.Union(city.borderTiles).ToList();
        for (int i = 0; i < tilesToCheck.Count; i++) {
            List<HexTile> neighbours = tilesToCheck[i].AllNeighbours;
            for (int j = 0; j < neighbours.Count; j++) {
                HexTile currNeighbour = neighbours[j];
                if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                    && currNeighbour.ownedByCity.kingdom.id != this.id) {
                    Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;
					KingdomManager.Instance.DiscoverKingdom (this, otherKingdom);
                } else if (currNeighbour.isBorder) {
                    for (int k = 0; k < currNeighbour.isBorderOfCities.Count; k++) {
                        Kingdom otherKingdom = currNeighbour.isBorderOfCities[k].kingdom;
                        if (otherKingdom.id != this.id && !this.discoveredKingdoms.Contains(otherKingdom)) {
                            KingdomManager.Instance.DiscoverKingdom(this, otherKingdom);
                        }
                    }
                }
            }
        }
    }
    internal void DiscoverKingdom(Kingdom discoveredKingdom) {
        if(discoveredKingdom.id != this.id) {
            if (!this._discoveredKingdoms.Contains(discoveredKingdom)) {
                this._discoveredKingdoms.Add(discoveredKingdom);
                Debug.Log(this.name + " discovered " + discoveredKingdom.name + "!");
            }
        }
    }
    internal void RemoveKingdomFromDiscoveredKingdoms(Kingdom kingdomToRemove) {
        for (int i = 0; i < _discoveredKingdoms.Count; i++) {
            Kingdom currKingdom = _discoveredKingdoms[i];
            if(currKingdom.id == kingdomToRemove.id) {
                _discoveredKingdoms.RemoveAt(i);
                break;
            }
        }
        //this._discoveredKingdoms.Remove(kingdomToRemove);
    }
    #endregion

    #region Bioweapon
    internal void SetBioWeapon(bool state){
		this._hasBioWeapon = state;
	}
	#endregion


	internal bool HasWar(Kingdom exceptionKingdom = null){
		if(exceptionKingdom == null){
			if(this._warfareInfo.Count > 0){
				return true;
			}
		}else{
			foreach (KingdomRelationship relationship in relationships.Values) {
				if (relationship.sharedRelationship.isAtWar && exceptionKingdom.id != relationship.targetKingdom.id) {
					return true;
				}
			}
		}

		return false;
	}

	internal Citizen GetRandomGovernorFromKingdom(){
		City randomCity = this.cities [UnityEngine.Random.Range (0, this.cities.Count)];
		return randomCity.governor;
	}

    #region Fog Of War
    internal void SetFogOfWarStateForRegion(Region region, FOG_OF_WAR_STATE fowState) {
        _regionFogOfWarDict[region] = fowState;
        for (int i = 0; i < region.tilesInRegion.Count; i++) {
            SetFogOfWarStateForTile(region.tilesInRegion[i], fowState);
        }
        for (int i = 0; i < region.outerGridTilesInRegion.Count; i++) {
            SetFogOfWarStateForTile(region.outerGridTilesInRegion[i], fowState, true);
        }
    }
    internal void SetFogOfWarStateForTile(HexTile tile, FOG_OF_WAR_STATE fowState, bool isOuterGridTile = false) {
        FOG_OF_WAR_STATE previousStateOfTile = FOG_OF_WAR_STATE.HIDDEN;
        if (isOuterGridTile) {
            previousStateOfTile = _outerFogOfWar[tile];
        } else {
            previousStateOfTile = _fogOfWar[tile.xCoordinate, tile.yCoordinate];
        }
        //Remove tile from previous list that it belonged to
        _fogOfWarDict[previousStateOfTile].Remove(tile);

        if (isOuterGridTile) {
            //Set new state of tile in outer grid dictionary
            _outerFogOfWar[tile] = fowState;
        } else {
            //Set new state of tile in fog of war dictionary
            _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        }
        
        //Check if tile is already in the list
        if (!_fogOfWarDict[fowState].Contains(tile)) {
            //if not, add it to the new states list
            _fogOfWarDict[fowState].Add(tile);
        }

        //if this kingdom is currently the active kingdom, automatically update the tile visual
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UpdateFogOfWarVisualForTile(tile, fowState);
        }

        //For checking if tile dictionary is accurate, remove this when checking is no longer necessary
        int sum = _fogOfWarDict.Sum(x => x.Value.Count);
        if (sum != GridMap.Instance.listHexes.Count + GridMap.Instance.outerGridList.Count) {
            throw new Exception("Fog of war dictionary is no longer accurate!");
        }
    }
    internal void UpdateFogOfWarVisual() {
        for (int x = 0; x < fogOfWar.GetLength(0); x++) {
            for (int y = 0; y < fogOfWar.GetLength(1); y++) {
                FOG_OF_WAR_STATE fowStateToUse = fogOfWar[x, y];
                HexTile currHexTile = GridMap.Instance.map[x, y];
                UpdateFogOfWarVisualForTile(currHexTile, fowStateToUse);
            }
        }
        foreach (KeyValuePair<HexTile, FOG_OF_WAR_STATE> kvp in _outerFogOfWar) {
            UpdateFogOfWarVisualForTile(kvp.Key, kvp.Value);
        }
    }
    private void UpdateFogOfWarVisualForTile(HexTile hexTile, FOG_OF_WAR_STATE fowState) {
        hexTile.SetFogOfWarState(fowState);
    }
	//internal FOG_OF_WAR_STATE GetFogOfWarStateOfTile(HexTile hexTile){
	//	return this._fogOfWar [hexTile.xCoordinate, hexTile.yCoordinate];
	//}
    #endregion

	internal int GetNumberOfWars(){
		int numOfWars = 0;
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			if(relationship.sharedRelationship.isAtWar){
				numOfWars += 1;
			}
		}
		if(numOfWars > 0){
			numOfWars -= 1;
		}
		return numOfWars;
	}
		
	internal void SetLockDown(bool state){
		this._isLockedDown = state;
	}

	internal void SetTechProduction(bool state){
		this._isTechProducing = state;
	}
	internal void SetTechProductionPercentage(float amount){
		this._techProductionPercentage = amount;
	}
	internal void SetProductionGrowthPercentage(float amount){
		this._productionGrowthPercentage = amount;
	}
	internal void SetSecession(bool state){
		this._hasSecession = state;
	}

	#region Crimes
	private void NewRandomCrimeDate(bool isFirst = false){
		int month = 0;
		int day = 0;
		if(isFirst){
			month = UnityEngine.Random.Range (1, 5);
			day = UnityEngine.Random.Range (1, GameManager.daysInMonth [month] + 1);
		}else{
			int lowerBoundMonth = this._crimeDate.month + 3;
			int upperBoundMonth = lowerBoundMonth + 1;

			month = UnityEngine.Random.Range (lowerBoundMonth, upperBoundMonth + 1);
			if(month > 12){
				month -= 12;
			}
			day = UnityEngine.Random.Range (1, GameManager.daysInMonth [month] + 1);
		}
		this._crimeDate.month = month;
		this._crimeDate.day = day;
	} 
	private void TriggerCrime(){
		if(!this.isDead){
			CreateCrime ();
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (3);
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				gameDate.AddMonths (1);
			}
			gameDate.day = UnityEngine.Random.Range (1, GameManager.daysInMonth [gameDate.month]);

			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerCrime());
		}
//		if(GameManager.Instance.month == this._crimeDate.month && GameManager.Instance.days == this._crimeDate.day){
//			NewRandomCrimeDate ();
//			CreateCrime ();
//		}
	}
	private void CreateCrime(){
		CrimeData crimeData = CrimeEvents.Instance.GetRandomCrime ();
		EventCreator.Instance.CreateCrimeEvent (this, crimeData);
	}
    #endregion

    #region Event Management
    internal void AddActiveEvent(GameEvent gameEvent) {
        this.activeEvents.Add(gameEvent);
    }
    internal void RemoveActiveEvent(GameEvent gameEvent) {
        this.activeEvents.Remove(gameEvent);
        AddToDoneEvents(gameEvent);
    }
    internal void AddToDoneEvents(GameEvent gameEvent) {
        this.doneEvents.Add(gameEvent);
        if (this.doneEvents.Count > KingdomManager.Instance.maxKingdomEventHistory) {
            this.doneEvents.RemoveAt(0);
        }
    }
    internal bool HasActiveEvent(EVENT_TYPES eventType) {
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                return true;
            }
        }
        return false;
    }
    internal int GetActiveEventsOfTypeCount(EVENT_TYPES eventType) {
        int count = 0;
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                count += 1;
            }
        }
        return count;
    }
    internal List<GameEvent> GetEventsOfType(EVENT_TYPES eventType, bool isActiveOnly = true) {
        List<GameEvent> gameEvents = new List<GameEvent>();
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                gameEvents.Add(this.activeEvents[i]);
            }
        }
        if (!isActiveOnly) {
            for (int i = 0; i < this.doneEvents.Count; i++) {
                if (this.doneEvents[i].eventType == eventType) {
                    gameEvents.Add(this.doneEvents[i]);
                }
            }
        }
        return gameEvents;
    }
    #endregion

    #region Governors Loyalty/Opinion
    internal void HasConflicted(GameEvent gameEvent){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).AddEventModifier (-10, "Recent border conflict", gameEvent);
			}
		}
	}
    #endregion

    #region Weapon Functions
    internal int GetWeaponOverProductionPercentage() {
        float overProductionPercentage = ((float)_baseWeapons / (float)populationCapacity) * 100f;
        overProductionPercentage -= 100;
        overProductionPercentage = Mathf.Clamp(overProductionPercentage, 0f, 100f);
        return Mathf.FloorToInt(overProductionPercentage);
    }
    internal void AdjustBaseWeapons(int amountToAdjust) {
        this._baseWeapons += amountToAdjust;
        if (this._baseWeapons < 0) {
            this._baseWeapons = 0;
        }
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void SetBaseWeapons(int newBaseWeapons) {
        _baseWeapons = newBaseWeapons;
        KingdomManager.Instance.UpdateKingdomList();
    }
    #endregion

    #region Stability Functions
    internal void AdjustStability(int amountToAdjust) {
        this._stability += amountToAdjust;
        this._stability = Mathf.Clamp(this._stability, -100, 100);
    }
    internal void CheckStability() {
        if (this.isDead) {
            return;
        }
        //A Rebellion will occur when Stability reaches -100. Stability will reset to 50. 
        if (_stability <= -100 && kingdomSize != KINGDOM_SIZE.SMALL) {
            StartAutomaticRebellion();
        }

        ////When Stability reaches -100, there will either be a rebellion, a plague or rioting.
        //if (_stability <= -100) {
        //    if (kingdomSize != KINGDOM_SIZE.SMALL) {
        //        //Large or medium kingdom
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 15) {
        //            StartAutomaticRebellion();
        //        } else if (chance >= 15 && chance < 35) {
        //            EventCreator.Instance.CreatePlagueEvent(this);
        //        } else if (chance >= 35 && chance < 60) {
        //            EventCreator.Instance.CreateRiotEvent(this);
        //        } else if (chance >= 60 && chance < 85) {
        //            EventCreator.Instance.CreateRiotSettlementEvent(this);
        //        } else {
        //            EventCreator.Instance.CreateRegressionEvent(this);
        //        }
        //    } else {
        //        //small kingdom
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 25) {
        //            EventCreator.Instance.CreatePlagueEvent(this);
        //        } else if (chance >= 25 && chance < 55) {
        //            EventCreator.Instance.CreateRiotEvent(this);
        //        } else if (chance >= 55 && chance < 85) {
        //            EventCreator.Instance.CreateRiotSettlementEvent(this);
        //        } else {
        //            EventCreator.Instance.CreateRegressionEvent(this);
        //        }
        //    }
        //}
    }
    internal void ChangeStability(int newAmount) {
        this._stability = newAmount;
        this._stability = Mathf.Clamp(this._stability, -100, 100);
        CheckStability();
    }
    internal int GetMonthlyStabilityGain() {
        int totalStabilityIncrease = GetStabilityContributionFromCitizens();
        //totalStabilityIncrease = Mathf.FloorToInt(totalStabilityIncrease * (1f - draftRate));
        //for (int i = 0; i < cities.Count; i++) {
        //    City currCity = cities[i];
        //    if (!currCity.isDead) {
        //        int weaponsContribution = 0;
        //        int armorContribution = 0;
        //        currCity.MonthlyResourceBenefits(ref weaponsContribution, ref armorContribution, ref totalStabilityIncrease);
        //    }
        //}
        //totalStabilityIncrease -= (_stabilityDecreaseFromInvasionCounter * 2);

        //Stability has a -5 monthly reduction when the Kingdom is Medium and a -10 monthly reduction when the Kingdom is Large
        if (kingdomSize == KINGDOM_SIZE.MEDIUM) {
            totalStabilityIncrease -= 1;
        } else if (kingdomSize == KINGDOM_SIZE.LARGE) {
            totalStabilityIncrease -= 2;
        }

        return totalStabilityIncrease;
    }
    internal void AddStabilityDecreaseBecauseOfInvasion() {
        _stabilityDecreaseFromInvasionCounter += 1;
        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(6);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => ReduceStabilityDecreaseBecauseOfInvasion());
        datesStabilityDecreaseWillExpire.Add(dueDate);
    }
    private void ReduceStabilityDecreaseBecauseOfInvasion() {
        datesStabilityDecreaseWillExpire.RemoveAt(0);
        _stabilityDecreaseFromInvasionCounter -= 1;
    }
    private int GetStabilityContributionFromCitizens() {
        return king.GetStabilityContribution();
    }
    #endregion

    #region Balance of Power
    internal void Militarize(bool state, bool isAttacking = false){
		this._isMilitarize = state;
		if(UIManager.Instance.currentlyShowingKingdom.id == this.id){
			UIManager.Instance.militarizingGO.SetActive (state);
		}
//        if (state) {
//            Kingdom kingdom2 = null;
//            float highestInvasionValue = -1;
//			bool isAtWar = false;
//            foreach (KingdomRelationship kr in relationships.Values) {
//                if (kr.isDiscovered) {
//                    if(kr.targetKingdomInvasionValue > highestInvasionValue) {
//                        kingdom2 = kr.targetKingdom;
//                        highestInvasionValue = kr.targetKingdomInvasionValue;
//                    }
//                }
//				if(kr.isAtWar && isAttacking){
//					isAtWar = true;
//					kingdom2 = kr.targetKingdom;
//					break;
//				}
//            }
//            if (kingdom2 != null) {
//				string militarizeFileName = "militarize";
//				if(isAttacking && isAtWar){
//					militarizeFileName = "militarize_attack";
//				}
//				Log militarizeLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", militarizeFileName);
//                militarizeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
//                militarizeLog.AddToFillers(kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
//                UIManager.Instance.ShowNotification(militarizeLog, new HashSet<Kingdom>() { this }, false);
//            }
//        }
    }
	internal void Fortify(bool state, bool isUnderAttack = false){
		this._isFortifying = state;
		if(UIManager.Instance.currentlyShowingKingdom.id == this.id){
			UIManager.Instance.fortifyingGO.SetActive (state);
		}
		if (state) {
            Kingdom kingdom2 = null;
            float highestKingdomThreat = -1;
			bool isAtWar = false;
            foreach (KingdomRelationship kr in relationships.Values) {
				if (kr.sharedRelationship.isDiscovered) {
                    if (kr.targetKingdomThreatLevel > highestKingdomThreat) {
                        kingdom2 = kr.targetKingdom;
                        highestKingdomThreat = kr.targetKingdomThreatLevel;
                    }
                }
				if(kr.sharedRelationship.isAtWar && isUnderAttack){
					isAtWar = true;
					kingdom2 = kr.targetKingdom;
					break;
				}
            }
            if (kingdom2 != null) {
				string fortifyFileName = "fortify";
				if(isUnderAttack && isAtWar){
					fortifyFileName = "fortify_under_attack";
				}
				Log fortifyLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", fortifyFileName);
                fortifyLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                fortifyLog.AddToFillers(kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
                UIManager.Instance.ShowNotification(fortifyLog, new HashSet<Kingdom>() { this }, false);
            }
		}
	}
	private void ScheduleActionDay(){
        this._evenActionDay = KingdomManager.Instance.GetEvenActionDay(this);
        //KingdomManager.Instance.IncrementEvenActionDay (2);
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, _evenActionDay, GameManager.Instance.year, () => ActionDay ());
		
	}
	private void ActionDay(){
		if(!this.isDead){
			UpdateThreatLevelsAndInvasionValues ();
            PerformWeightedAction();
			//if(this.race != RACE.UNDEAD){
			//	if (this.king.balanceType == PURPOSE.BALANCE) {
			//		SeeksBalance.Initialize(this);
			//	}else if (this.king.balanceType == PURPOSE.SUPERIORITY) {
			//		SeeksSuperiority.Initialize(this);
			//	}else if (this.king.balanceType == PURPOSE.BANDWAGON) {
			//		SeeksBandwagon.Initialize(this);
			//	}
			//}else{
			//	SeeksUndead.Initialize (this);
			//}

			CheckStability ();

			GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (1);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => ActionDay ());
		}
	}
    private void IncreaseBOPAttributesPerMonth() {
        if (this.isDead) {
            return;
        }
        int totalTechIncrease = GetTechContributionFromCitizens();
        //Kings and Governors provide monthly Stability gains based on their Efficiency trait.
        int totalStabilityIncrease = GetStabilityContributionFromCitizens();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead) {
                int techContribution = currCity.techPoints;
                totalTechIncrease += techContribution;
            }
        }

        //Stability has a -5 monthly reduction when the Kingdom is Medium and a -10 monthly reduction when the Kingdom is Large
        if (kingdomSize == KINGDOM_SIZE.MEDIUM) {
            totalStabilityIncrease -= 1;
        } else if(kingdomSize == KINGDOM_SIZE.LARGE) {
            totalStabilityIncrease -= 2;
        }

        AdjustStability(totalStabilityIncrease);

        //Tech Gains
        totalTechIncrease = ((2 * scientists * totalTechIncrease) / (scientists + totalTechIncrease));
        totalTechIncrease = Mathf.FloorToInt(totalTechIncrease * techProductionPercentage);
        AdjustTechCounter(totalTechIncrease);

        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseBOPAttributesPerMonth());

        CheckStability();
    }

    internal void UpdateProductionRatesFromKing() {
        _researchRateFromKing = 0f;
        _draftRateFromKing = 0f;
        _productionRateFromKing = 0f;

        //switch (king.science) {
        //    case SCIENCE.ERUDITE:
        //        _researchRateFromKing = 0.10f;
        //        break;
        //    case SCIENCE.ACADEMIC:
        //        _researchRateFromKing = 0.05f;
        //        break;
        //    case SCIENCE.IGNORANT:
        //        _researchRateFromKing = -0.05f;
        //        break;
        //    default:
        //        break;
        //}
        //_productionRateFromKing -= _researchRateFromKing;

        switch (king.military) {
            case TRAIT.HOSTILE:
                _draftRateFromKing = 0.10f;
                break;
            //case TRAIT.MILITANT:
            //    _draftRateFromKing = 0.05f;
            //    break;
            case TRAIT.PACIFIST:
                _draftRateFromKing = -0.05f;
                break;
            default:
                break;
        }
        _productionRateFromKing -= _draftRateFromKing;


    }
    #endregion

    #region Adjacency
    internal void AddAdjacentKingdom(Kingdom kingdom) {
        if (!_adjacentKingdoms.Contains(kingdom)) {
            this._adjacentKingdoms.Add(kingdom);
        }
    }
    internal void RemoveAdjacentKingdom(Kingdom kingdom) {
        this._adjacentKingdoms.Remove(kingdom);
    }
    internal void RevalidateKingdomAdjacency(City removedCity) {
        List<Kingdom> kingdomsToCheck = new List<Kingdom>();
        for (int i = 0; i < removedCity.region.adjacentRegionsViaMajorRoad.Count; i++) {
            Region currRegion = removedCity.region.adjacentRegionsViaMajorRoad[i];
            if (currRegion.occupant != null) {
                if (currRegion.occupant.kingdom != this && !kingdomsToCheck.Contains(currRegion.occupant.kingdom)) {
                    kingdomsToCheck.Add(currRegion.occupant.kingdom);
                }
            }
        }
        for (int i = 0; i < kingdomsToCheck.Count; i++) {
            Kingdom otherKingdom = kingdomsToCheck[i];
            KingdomRelationship kr = GetRelationshipWithKingdom(otherKingdom, true);
			if (kr != null && kr.sharedRelationship.isAdjacent) {
                bool isValid = false;
                //Revalidate adjacency
                for (int j = 0; j < cities.Count; j++) {
                    Region regionOfCurrCity = cities[j].region;
                    foreach (Region otherRegion in regionOfCurrCity.adjacentRegionsViaMajorRoad.Where(x => x.occupant != null && x.occupant.kingdom.id != this.id)) {
                        if (otherRegion.occupant.kingdom.id == otherKingdom.id) {
                            //otherKingdom is still adjacent to this kingdom, validity verified!
                            isValid = true;
                            break;
                        } 

                    }
                    if (isValid) {
                        //otherKingdom has already been verified! Skip checking of other cities
                        break;
                    }
                }
                //Loop of all cities is done, check if validity has returned a true value,
                //if not, this kingdom and other kingdom are no longer adjacent, change appropriately
                if (!isValid) {
                    kr.ChangeAdjacency(false);
                }
            }
        }

    }
    #endregion

    #region Population
//	internal void AdjustPopulation(float adjustment, bool isUpdateKingdomList = true) {
//        _population += adjustment;
//        if(_population < 0f) {
//			_population = 0f;
//        }
//		if(isUpdateKingdomList){
//			KingdomManager.Instance.UpdateKingdomList();
//		}
//    }
//	internal void SetPopulation(float newPopulation) {
//        _population = newPopulation;
//        KingdomManager.Instance.UpdateKingdomList();
//    }
//	internal void UpdatePopulation(){
//		this._population = 0f;
//		for (int i = 0; i < this.cities.Count; i++) {
//			this._population += this.cities [i]._population;
//		}
//		KingdomManager.Instance.UpdateKingdomList ();
//	}
	internal void DamagePopulation(int damage){
		int distributableDamage = damage / this.cities.Count;
		int remainder = damage % this.cities.Count;
		distributableDamage += remainder;
		int excessDamage = 0;
		for (int i = 0; i < this.cities.Count; i++) {
			City city = this.cities [i];
			int totalDamage = distributableDamage + excessDamage;
			if(city.population < totalDamage){
				excessDamage += totalDamage - city.population;
				city.AdjustPopulation ((float)-city.population);
				i--;
			}else{
				excessDamage = 0;
				city.AdjustPopulation ((float)-totalDamage);
				if(city.isDead){
					i--;
				}
			}
		}
	}
    #endregion

    #region Automatic Rebellion
    private void StartAutomaticRebellion() {
        //If a Kingdom has a -100 Stability, a Rebellion will automatically occur which will be started by the one with the most 
        //negative opinion towards the King. The Kingdom's Stability will then reset back to 50.
        List<Citizen> possibleCitizensForRebellion = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            possibleCitizensForRebellion.AddRange(cities[i].citizens.Where(x => x.role != ROLE.KING && x.role != ROLE.UNTRAINED));
        }
        if (possibleCitizensForRebellion.Count > 0) {
            possibleCitizensForRebellion.OrderBy(x => x.loyaltyToKing).First().StartRebellion();
        }
    }
    #endregion

    internal void MobilizingState(bool state){
		this._isMobilizing = state;
	}

	internal void AdjustWarmongerValue(int amount){
		this._warmongerValue += amount;
		this._warmongerValue = Mathf.Clamp(this._warmongerValue, 0, 200);
	}

	internal void SetWarmongerValue(int amount){
		this._warmongerValue = amount;
	}

	internal void WarmongerDecreasePerYear(){
		if(!this.isDead){
			if (!HasWar ()) {
				AdjustWarmongerValue (-5);
			}
			SchedulingManager.Instance.AddEntry (1, 1, GameManager.Instance.year + 1, () => WarmongerDecreasePerYear ());
		}
	}
	internal void UpdateThreatLevelsAndInvasionValues(){
		has100OrAboveThreat = false;
		if(this.race == RACE.UNDEAD){
			foreach (KingdomRelationship relationship in this.relationships.Values) {
				relationship.UpdateThreatLevel ();
				relationship.UpdateLikeness ();
			}
		}else{
			if(this.king.balanceType == PURPOSE.BANDWAGON || this.king.balanceType == PURPOSE.SUPERIORITY){
				this.highestThreatAdjacentKingdomAbove50 = null;
				this.highestThreatAdjacentKingdomAbove50Value = 0f;
				this.highestRelativeStrengthAdjacentKingdom = null;
				this.highestRelativeStrengthAdjacentKingdomValue = 0;
				foreach (KingdomRelationship relationship in this.relationships.Values) {
					relationship.UpdateThreatLevel ();
				}
				foreach (KingdomRelationship relationship in this.relationships.Values) {
					relationship.UpdateLikeness ();
				}
			}else{
				foreach (KingdomRelationship relationship in this.relationships.Values) {
					relationship.UpdateThreatLevel ();
					relationship.UpdateLikeness ();
				}
			}
		}
	}

	internal void SeekAllianceOfProtection(){
		Kingdom kingdomWithHighestThreat = GetKingdomWithHighestThreat();
		if (kingdomWithHighestThreat != null) {
			Kingdom kingdomToAlly = null;
			int likeTheMost = 0;
			foreach (KingdomRelationship kr in this.relationships.Values) {
				if (kr.sharedRelationship.isDiscovered && !kr.sharedRelationship.cantAlly && !kr.sharedRelationship.isAtWar) {
					if (kr.targetKingdom.id != kingdomWithHighestThreat.id) {
						KingdomRelationship rk = kr.targetKingdom.GetRelationshipWithKingdom (kingdomWithHighestThreat);
						if (rk.sharedRelationship.isAdjacent) {
							if (kr.targetKingdom.alliancePool != null) {
								Debug.Log (name + " is looking to join the alliance of " + kr.targetKingdom.name);
								bool hasJoined = kr.targetKingdom.alliancePool.AttemptToJoinAlliance (this, kr.targetKingdom);
								if (hasJoined) {
									string log = name + " has joined an alliance with ";
									for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
										if (_alliancePool.kingdomsInvolved [j].id != id) {
											log += _alliancePool.kingdomsInvolved [j].name;
											if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
												log += ", ";
											}
										}
									}
									Debug.Log(log);
									return;
								}
							} else {
								if(kingdomToAlly == null){
									kingdomToAlly = kr.targetKingdom;
									likeTheMost = kr.totalLike;
								}else{
									if(kr.totalLike > likeTheMost){
										kingdomToAlly = kr.targetKingdom;
										likeTheMost = kr.totalLike;
									}
								}
							}

						}
					}
				}
			}
			if (kingdomToAlly != null) {
				Debug.Log(name + " is looking to create an alliance with " + kingdomToAlly.name);
				bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kingdomToAlly);
				if(hasCreated){
					string log = name + " has created an alliance with ";
					for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
						if(_alliancePool.kingdomsInvolved[j].id != id) {
							log += _alliancePool.kingdomsInvolved[j].name;
							if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
								log += ", ";
							}
						}
					}
					Debug.Log(log);
				}
			}
		}
		if(this._alliancePool == null){
			Kingdom kingdomToAlly = null;
			int likeTheMost = 0;
			foreach (KingdomRelationship kr in this.relationships.Values) {			
				if(kr.sharedRelationship.isDiscovered && !kr.sharedRelationship.cantAlly && !kr.sharedRelationship.isAtWar){
					if (kr.targetKingdom.alliancePool != null) {
						Debug.Log (name + " is looking to join the alliance of " + kr.targetKingdom.name);
						bool hasJoined = kr.targetKingdom.alliancePool.AttemptToJoinAlliance (this, kr.targetKingdom);
						if (hasJoined) {
							string log = name + " has joined an alliance with ";
							for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
								if (_alliancePool.kingdomsInvolved [j].id != id) {
									log += _alliancePool.kingdomsInvolved [j].name;
									if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
										log += ", ";
									}
								}
							}
							return;
						}
					} else {
						if (kingdomToAlly == null) {
							kingdomToAlly = kr.targetKingdom;
							likeTheMost = kr.totalLike;
						} else {
							if (kr.totalLike > likeTheMost) {
								kingdomToAlly = kr.targetKingdom;
								likeTheMost = kr.totalLike;
							}
						}
					}
				}
			}
			if (kingdomToAlly != null) {
				Debug.Log(name + " is looking to create an alliance with " + kingdomToAlly.name);
				bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kingdomToAlly);
				if(hasCreated){
					string log = name + " has created an alliance with ";
					for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
						if(_alliancePool.kingdomsInvolved[j].id != id) {
							log += _alliancePool.kingdomsInvolved[j].name;
							if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
								log += ", ";
							}
						}
					}
					Debug.Log(log);
				}
			}
		}

        if(this._alliancePool == null) {
            Debug.Log(name + " has failed to create/join an alliance");
        }
	}
	internal void SeekAllianceOfConquest(){
		Kingdom kingdomToAlly = null;
		int leastLikedToEnemy = 0;
		foreach (KingdomRelationship krToAlly in this.relationships.Values) {
			if (krToAlly.sharedRelationship.isDiscovered && !krToAlly.sharedRelationship.cantAlly && !krToAlly.sharedRelationship.isAtWar) {
				if (krToAlly.targetKingdom.id != this.highestRelativeStrengthAdjacentKingdom.id) {
					KingdomRelationship krFromAlly = krToAlly.targetKingdom.GetRelationshipWithKingdom (this);
					KingdomRelationship krEnemy = krToAlly.targetKingdom.GetRelationshipWithKingdom (this.highestRelativeStrengthAdjacentKingdom);
					if (krToAlly.totalLike > 0 && krFromAlly.totalLike > 0 && krEnemy.sharedRelationship.isAdjacent
						&& krToAlly.targetKingdom.king.balanceType == PURPOSE.SUPERIORITY && KingdomManager.Instance.kingdomRankings [0].id != krToAlly.targetKingdom.id && !krToAlly.sharedRelationship.cantAlly) {

						if (krToAlly.targetKingdom.alliancePool != null) {
							bool hasJoined = krToAlly.targetKingdom.alliancePool.AttemptToJoinAlliance (this, krToAlly.targetKingdom);
							if (hasJoined) {
								string log = name + " has joined an alliance with ";
								for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
									if (_alliancePool.kingdomsInvolved [j].id != id) {
										log += _alliancePool.kingdomsInvolved [j].name;
										if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
											log += ", ";
										}
									}
								}
								Debug.Log (log);
								return;
							}
						} else {
							if (kingdomToAlly == null) {
								kingdomToAlly = krToAlly.targetKingdom;
								leastLikedToEnemy = krEnemy.totalLike;
							} else {
								if (krEnemy.totalLike < leastLikedToEnemy) {
									kingdomToAlly = krToAlly.targetKingdom;
									leastLikedToEnemy = krEnemy.totalLike;
								}
							}
						}
					}
				}
			}
		}
		if(kingdomToAlly != null){
			Debug.Log(name + " is looking to create an alliance with " + kingdomToAlly.name);
			bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kingdomToAlly);
			if(hasCreated){
				string log = name + " has created an alliance with ";
				for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
					if(_alliancePool.kingdomsInvolved[j].id != id) {
						log += _alliancePool.kingdomsInvolved[j].name;
						if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
							log += ", ";
						}
					}
				}
				Debug.Log(log);
			}
		}
	}
	internal void SetAlliancePool(AlliancePool alliancePool){
		this._alliancePool = alliancePool;
	}
	internal int GetUnadjacentPosAllianceWeapons(){
		int posAlliancePower = 0;
		if(this.alliancePool != null){
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this.alliancePool.kingdomsInvolved[i];
				if(this.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this);
					if(relationship.totalLike >= 35){
						posAlliancePower += (int)((float)kingdomInAlliance.baseWeapons * 0.1f);
					}
				}
			}
        }
		return posAlliancePower;
	}

	internal void AddWarfareInfo(WarfareInfo info){
		if(!this._warfareInfo.ContainsKey(info.warfare.id)){
			this._warfareInfo.Add(info.warfare.id, info);
		}
	}
	internal void RemoveWarfareInfo(Warfare warfare){
		this._warfareInfo.Remove(warfare.id);
	}
	internal WarfareInfo GetWarfareInfo(int id){
		if(this._warfareInfo.ContainsKey(id)){
			return this._warfareInfo[id];
		}
		return new WarfareInfo(WAR_SIDE.NONE, null);
	}
	internal void LeaveAlliance(bool doNotShowLog = false){
		if(this.alliancePool != null){
            AlliancePool leftAlliance = this.alliancePool;
			object[] objects = leftAlliance.kingdomsInvolved.ToArray ();
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				if(this.alliancePool.kingdomsInvolved[i].id != this.id){
                    Kingdom otherKingdom = this.alliancePool.kingdomsInvolved[i];
					KingdomRelationship kr = otherKingdom.GetRelationshipWithKingdom (this);
					kr.AddRelationshipModifier (-50, "Broken Alliance", RELATIONSHIP_MODIFIER.LEAVE_ALLIANCE, true, false);
					kr.ChangeCantAlly (true);
                    AddRecentlyBrokenAllianceWith(otherKingdom);
                }
			}
            this.alliancePool.RemoveKingdomInAlliance(this);
			//When leaving an alliance, Stability is reduced by 15
			this.AdjustStability(-5);
			if(!doNotShowLog){
				Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Alliance", "leave_alliance");
				newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                newLog.AddToFillers(null, leftAlliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
				newLog.AddAllInvolvedObjects (objects);
				UIManager.Instance.ShowNotification (newLog);
			}
		}
	}
	internal bool IsUnderAttack(){
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].isDefending){
				return true;
			}
		}
		return false;
	}
	internal bool IsAttacking(){
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].isAttacking){
				return true;
			}
		}
		return false;
	}
	internal string ProvideWeaponsArmorsAidToKingdom(Kingdom kingdomToBeProvided, float transferPercentage){
		int transferAmount = (int)(this.baseWeapons * transferPercentage);
		this.AdjustBaseWeapons (-transferAmount);
		kingdomToBeProvided.AdjustBaseWeapons (transferAmount);
		return transferAmount.ToString () + " Weapons";
	}
	internal Kingdom GetKingdomWithHighestThreat(){
		float highestThreatLevel = 0f;
		Kingdom threat = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			if(kr.targetKingdomThreatLevel > highestThreatLevel){
				highestThreatLevel = kr.targetKingdomThreatLevel;
				threat = kr.targetKingdom;
			}
		}
		return threat;
	}
	internal void ShowTransferWeaponsArmorsLog(Kingdom allyKingdom, string amount){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        foreach (WarfareInfo currWarFare in this.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        kingdomsToShowNotif.Add(allyKingdom);
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "transfer_weapons_armors");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (allyKingdom, allyKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, amount, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowJoinWarLog(Kingdom allyKingdom, Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        foreach (WarfareInfo currWarFare in allyKingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "join_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (allyKingdom, allyKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowRefuseAndLeaveAllianceLog(AlliancePool alliance, Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(this);
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "refuse_and_leave_alliance");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		newLog.AddAllInvolvedObjects (alliance.kingdomsInvolved.ToArray ());
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowDoNothingLog(Warfare warfare){
		Debug.Log(this.name + " decided to do nothing with " + warfare.name);

        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "do_nothing_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalWarLog(Warfare warfare, Kingdom kingdom){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(kingdom);
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        foreach (WarfareInfo currWarFare in kingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalLog(AlliancePool alliance, Kingdom kingdom){
		List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
		kingdomsToShowNotif.Add(kingdom);
		kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);

		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddAllInvolvedObjects (alliance.kingdomsInvolved.ToArray ());
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalProvideLog(AlliancePool alliance, string logAmount, Kingdom kingdom){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(this);
        kingdomsToShowNotif.Add(kingdom);
        kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);

        foreach (WarfareInfo currWarFare in this.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        foreach (WarfareInfo currWarFare in kingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_provide");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		newLog.AddToFillers (null, logAmount, LOG_IDENTIFIER.OTHER);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}

	internal Kingdom GetKingdomWithHighestAtLeastAbove50Threat(){
		float highestThreat = 0f;
		Kingdom highestThreatKingdom = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			float threat = kr.targetKingdomThreatLevel;
			if(kr.sharedRelationship.isAdjacent && threat > 50f){
				if(highestThreatKingdom == null){
					highestThreatKingdom = kr.targetKingdom;
					highestThreat = threat;
				}else{
					if(threat > highestThreat){
						highestThreatKingdom = kr.targetKingdom;
						highestThreat = threat;
					}
				}
			}
		}
		return highestThreatKingdom;
	}
	private Kingdom GetAdjacentKingdomLeastLike(){
		int leastLike = 0;
		Kingdom targetKingdom = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			if(kr.sharedRelationship.isAdjacent && !kr.AreAllies() && kr.totalLike < 0){
				if(targetKingdom == null){
					targetKingdom = kr.targetKingdom;
					leastLike = kr.totalLike;
				}else{
					if(kr.totalLike < leastLike){
						targetKingdom = kr.targetKingdom;
						leastLike = kr.totalLike;
					}
				}
			}
		}
		return targetKingdom;
	}
	internal int GetSumOfCityLevels(){
		return this.cities.Sum (x => x.cityLevel);
	}

	#region Subterfuge
	internal void Subterfuge(){
		Kingdom targetKingdom = GetAdjacentKingdomLeastLike ();
		if(targetKingdom != null){
			int triggerChance = UnityEngine.Random.Range (0, 100);
			int triggerValue = 5;
			//if(this.king.loyalty == LOYALTY.SCHEMING){
			//	triggerValue += 3;
			//}
			if(triggerChance < triggerValue){
				SUBTERFUGE_ACTIONS subterfuge = GetSubterfugeAction ();
				CreateSubterfugeEvent (subterfuge, targetKingdom);
			}
		}
	}
	internal void CreateSubterfugeEvent(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		bool noCaughtAction = false;
		int successChance = UnityEngine.Random.Range (0, 100);
		int caughtChance = UnityEngine.Random.Range (0, 100);
		int successValue = 60;
		if(this.king.HasTrait(TRAIT.SMART)){
			successValue += 15;
		}else if(this.king.HasTrait(TRAIT.DUMB)){
			successValue -= 15;
		}
		if(successChance < successValue){
			//Success
			CreateSuccessSubterfugeAction(subterfuge, targetKingdom);

		}else{
			//Fail
			int criticalFailChance = UnityEngine.Random.Range (0, 100);
			int criticalFailValue = 20;
			if(this.king.HasTrait(TRAIT.INEPT)){
				criticalFailValue += 5;
			}else if(this.king.HasTrait(TRAIT.EFFICIENT)){
				criticalFailValue -= 5;
			}
			if(criticalFailChance < criticalFailValue){
				//Critical Failure
				if(subterfuge == SUBTERFUGE_ACTIONS.INTERNATIONAL_INCIDENT){
					noCaughtAction = true;
				}
				CreateCriticalFailSubterfugeAction(subterfuge, targetKingdom);
			}else{
				//Failure
				CreateFailSubterfugeAction(subterfuge, targetKingdom);
			}
		}

		if(!noCaughtAction){
			if(caughtChance < 5){
				CreateCaughtSubterfugeAction (subterfuge, targetKingdom);
			}
		}
	}
	private SUBTERFUGE_ACTIONS GetSubterfugeAction(){
		int chance = UnityEngine.Random.Range (0, 100);

		if (this.king.balanceType == PURPOSE.BALANCE) {
			if(chance < 40){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else if(chance >= 40 && chance < 70){
				return SUBTERFUGE_ACTIONS.REDUCE_STABILITY;
			}else if(chance >= 70 && chance < 90){
				return SUBTERFUGE_ACTIONS.FLATTER;
			}else{
				return SUBTERFUGE_ACTIONS.SPREAD_PLAGUE;
			}
		}

		if (this.king.balanceType == PURPOSE.SUPERIORITY) {
			if(chance < 40){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else if(chance >= 40 && chance < 70){
				return SUBTERFUGE_ACTIONS.REDUCE_STABILITY;
			}else if(chance >= 70 && chance < 90){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else{
				return SUBTERFUGE_ACTIONS.SPREAD_PLAGUE;
			}
		}

		if (this.king.balanceType == PURPOSE.BANDWAGON) {
			if(chance < 40){
				return SUBTERFUGE_ACTIONS.FLATTER;
			}else if(chance >= 40 && chance < 70){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else if(chance >= 70 && chance < 90){
				return SUBTERFUGE_ACTIONS.SPREAD_PLAGUE;
			}else{
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}
		}

		return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
	}

	private void CreateSuccessSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		if(subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS){
			int weaponsDestroyed = targetKingdom.DestroyWeaponsSubterfuge ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom, weaponsDestroyed);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.REDUCE_STABILITY){
			targetKingdom.InciteUnrestSubterfuge ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.FLATTER){
			FlatterSubterfuge (targetKingdom, 50);
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE){
			string plagueName = targetKingdom.SpreadPlague ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom, 0, plagueName);
        } else if(subterfuge == SUBTERFUGE_ACTIONS.INTERNATIONAL_INCIDENT) {
            StartInternationalIncident(targetKingdom, "success");
        }
	}
	private void CreateCriticalFailSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		if (subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS) {
			int weaponsDestroyed = DestroyWeaponsSubterfuge ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom, weaponsDestroyed);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.REDUCE_STABILITY){
			InciteUnrestSubterfuge ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.FLATTER){
			FlatterSubterfuge (targetKingdom, -50);
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE){
			string plagueName = SpreadPlague ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom, 0, plagueName);
        } else if (subterfuge == SUBTERFUGE_ACTIONS.INTERNATIONAL_INCIDENT) {
			StartInternationalIncident(targetKingdom, "critical_fail");
        }
    }
	private void CreateFailSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
        if (subterfuge == SUBTERFUGE_ACTIONS.INTERNATIONAL_INCIDENT) {
            StartInternationalIncident(targetKingdom, "fail");
        } else {
            ShowFailSubterfugeLog(subterfuge, targetKingdom);
        }
	}
	private void CreateCaughtSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
        if (subterfuge == SUBTERFUGE_ACTIONS.INTERNATIONAL_INCIDENT) {
            StartInternationalIncident(targetKingdom, "caught");
        }
        //InternationalIncident intlIncident = EventCreator.Instance.CreateInternationalIncidentEvent(this, targetKingdom, this, true, false);
        //		ShowCaughtSubterfugeLog (subterfuge, targetKingdom, intlIncident.incidentName);
    }
	private int DestroyWeaponsSubterfuge(){
		int weaponsToBeDestroyed = (int)((float)this._baseWeapons * 0.05f);
		this.AdjustBaseWeapons (-weaponsToBeDestroyed);
		return weaponsToBeDestroyed;
	}
	private void InciteUnrestSubterfuge(){
		this.AdjustStability (-10);
	}
	private void FlatterSubterfuge(Kingdom targetKingdom, int modifier){
		KingdomRelationship kr = targetKingdom.GetRelationshipWithKingdom (this);
		kr.AddRelationshipModifier (modifier, "Flatter", RELATIONSHIP_MODIFIER.FLATTER, true, false);
	}
	private string SpreadPlague(){
		Plague plague = EventCreator.Instance.CreatePlagueEvent(this, false);
		return plague._plagueName;
	}
	private void AssassinateKing(Kingdom targetKingdom){
		
	}
	private void ShowSuccessSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, int weaponsArmorsDestroyed = 0, string plagueName = ""){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", subterfuge.ToString() + "_SUCCESS");
		newLog.AddToFillers (this.king, this.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		if (subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS) { //subterfuge == SUBTERFUGE_ACTIONS.DESTROY_ARMORS || 
			newLog.AddToFillers (null, weaponsArmorsDestroyed.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE) {
			newLog.AddToFillers (null, plagueName.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.FLATTER) {
			newLog.AddToFillers (targetKingdom.king, targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
		UIManager.Instance.ShowNotification (newLog);
	}
	private void ShowFailSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, int weaponsArmorsDestroyed = 0, string plagueName = ""){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", subterfuge.ToString() + "_FAIL");
		newLog.AddToFillers (this.king, this.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		if (subterfuge == SUBTERFUGE_ACTIONS.FLATTER) {
			newLog.AddToFillers (targetKingdom.king, targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
		UIManager.Instance.ShowNotification (newLog);
	}
	private void ShowCriticalFailSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, int weaponsArmorsDestroyed = 0, string plagueName = ""){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", subterfuge.ToString() + "_CRITICAL");
		newLog.AddToFillers (this.king, this.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		if (subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS) { //subterfuge == SUBTERFUGE_ACTIONS.DESTROY_ARMORS ||
			newLog.AddToFillers (null, weaponsArmorsDestroyed.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE) {
			newLog.AddToFillers (null, plagueName.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.FLATTER) {
			newLog.AddToFillers (targetKingdom.king, targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
		UIManager.Instance.ShowNotification (newLog);
	}
	private void ShowCaughtSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, string incidentName){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", "caught");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, incidentName, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification (newLog);
	}
	#endregion

	#region Undead
	internal void InitializeUndeadKingdom(int undeadCount){
//		this.AdjustPopulation (undeadCount);
	}
	#endregion

	#region Resources
	internal void AdjustFoodCityCapacity(int amount){
		this._foodCityCapacity += amount;
		if(this._foodCityCapacity < 0){
			this._foodCityCapacity = 0;
		}
	}
	internal void AdjustMaterialCityCapacityForHumans(int amount){
		this._materialCityCapacityForHumans += amount;
		if(this._materialCityCapacityForHumans < 0){
			this._materialCityCapacityForHumans = 0;
		}
	}
	internal void AdjustMaterialCityCapacityForElves(int amount){
		this._materialCityCapacityForElves += amount;
		if(this._materialCityCapacityForElves < 0){
			this._materialCityCapacityForElves = 0;
		}
	}
	internal void AdjustOreCityCapacity(int amount){
		this._oreCityCapacity += amount;
		if(this._oreCityCapacity < 0){
			this._oreCityCapacity = 0;
		}
	}
    internal Dictionary<RESOURCE_TYPE, int> GetSurplusResourcesFor(Kingdom otherKingdom) {
        Dictionary<RESOURCE_TYPE, int> surplusResources = new Dictionary<RESOURCE_TYPE, int>();

        if(this.cities.Count < this.foodCityCapacity) {
            surplusResources.Add(RESOURCE_TYPE.FOOD, this.foodCityCapacity - this.cities.Count);
        }

        if (otherKingdom.race == RACE.HUMANS) {
            if (this.cities.Count < this.materialCityCapacityForHumans) {
                surplusResources.Add(RESOURCE_TYPE.MATERIAL, this.materialCityCapacityForHumans - this.cities.Count);
            }
        } else if (otherKingdom.race == RACE.ELVES) {
            if (this.cities.Count < this._materialCityCapacityForElves) {
                surplusResources.Add(RESOURCE_TYPE.MATERIAL, this.materialCityCapacityForElves - this.cities.Count);
            }
        }
        if (this.cities.Count < this.oreCityCapacity) {
            surplusResources.Add(RESOURCE_TYPE.ORE, this.oreCityCapacity - this.cities.Count);
        }
        return surplusResources; 
    }
    internal Dictionary<RESOURCE_TYPE, int> GetDeficitResourcesFor(Kingdom otherKingdom) {
        Dictionary<RESOURCE_TYPE, int> deficitResources = new Dictionary<RESOURCE_TYPE, int>();

        if (this.cities.Count > this.foodCityCapacity) {
            deficitResources.Add(RESOURCE_TYPE.FOOD, this.cities.Count - this.foodCityCapacity);
        }

        if (otherKingdom.race == RACE.HUMANS) {
            if (this.cities.Count > this.materialCityCapacityForHumans) {
                deficitResources.Add(RESOURCE_TYPE.MATERIAL, this.cities.Count - this.materialCityCapacityForHumans);
            }
        } else if (otherKingdom.race == RACE.ELVES) {
            if (this.cities.Count > this._materialCityCapacityForElves) {
                deficitResources.Add(RESOURCE_TYPE.MATERIAL, this.cities.Count - this.materialCityCapacityForElves);
            }
        }
        if (this.cities.Count > this.oreCityCapacity) {
            deficitResources.Add(RESOURCE_TYPE.ORE, this.cities.Count - this.oreCityCapacity);
        }
        return deficitResources;
    }
    #endregion

    #region Goal Manager AI
    internal void PerformWeightedAction() {
        WEIGHTED_ACTION actionToPerform = GoalManager.Instance.DetermineWeightedActionToPerform(this);
        if (actionToPerform != WEIGHTED_ACTION.DO_NOTHING) {
            if (GoalManager.indirectActionTypes.Contains(actionToPerform)) {
                //This action type requires a return of 2 kingdoms, a target and a cause of the action
                Dictionary<Kingdom, Dictionary<Kingdom, int>> targetKingdomWeights = GoalManager.Instance.GetKingdomWeightsForIndirectActionType(this, actionToPerform);
                if (Utilities.GetTotalOfWeights(targetKingdomWeights) > 0) {
                    Debug.Log(Utilities.GetWeightsSummary(targetKingdomWeights, actionToPerform.ToString() + " weights: "));

                    Kingdom[] targets = Utilities.PickRandomElementWithWeights(targetKingdomWeights);
                    ClearRejectedOffersList(); //Clear List Since weights are already computed
                    if (targets.Length == 2) {
                        Kingdom cause = targets[0];
                        Kingdom target = targets[1];
                        Debug.Log(this.name + " has targeted " + target.name + " because of " + cause.name);
                        PerformAction(actionToPerform, target, cause);
                    } else {
                        Debug.Log(this.name + " tried to perform " + actionToPerform.ToString() + ", but it had no targets!", this.capitalCity.hexTile);
                    }
                } else {
                    ClearRejectedOffersList(); //Clear List Since weights are already computed
                    Debug.Log(this.name + " tried to perform " + actionToPerform.ToString() + ", but it had no targets!", this.capitalCity.hexTile);
                }
            } else if (GoalManager.specialActionTypes.Contains(actionToPerform)) {
                int weightToNotPerformAction = 0;
                if (actionToPerform == WEIGHTED_ACTION.LEAVE_ALLIANCE) {
                    List<AlliancePool> alliances = new List<AlliancePool>();
                    alliances.Add(alliancePool);
                    WeightedDictionary<AlliancePool> weights = GoalManager.Instance.GetWeightsForSpecialActionType(this, alliances, actionToPerform, ref weightToNotPerformAction);
                    ClearRejectedOffersList(); //Clear List Since weights are already computed
                    if (weights.GetTotalOfWeights() > 0) {
                        WeightedDictionary<WEIGHTED_ACTION> actionWeights = new WeightedDictionary<WEIGHTED_ACTION>();
                        actionWeights.AddElement(WEIGHTED_ACTION.DO_NOTHING, weightToNotPerformAction);
                        actionWeights.AddElement(actionToPerform, weights.GetTotalOfWeights());
                        actionWeights.LogDictionaryValues("Leave alliance weights: ");

                        WEIGHTED_ACTION decision = actionWeights.PickRandomElementGivenWeights();
                        Debug.Log(this.name + " has chosen to " + decision.ToString());
                        if (decision == actionToPerform) {
                            //this kingdom has decided to perform the action
                            PerformAction(actionToPerform, null);
                        }
                    } else {
                        Debug.Log(this.name + " tried to perform " + actionToPerform.ToString() + ", but it had no targets!", this.capitalCity.hexTile);
                    }
                } else if (actionToPerform == WEIGHTED_ACTION.LEAVE_TRADE_DEAL) {
                    WeightedDictionary<Kingdom> weights = GoalManager.Instance.GetWeightsForSpecialActionType(this, kingdomsInTradeDealWith, actionToPerform, ref weightToNotPerformAction);
                    ClearRejectedOffersList(); //Clear List Since weights are already computed
                    
                    if (weights.GetTotalOfWeights() > 0) {
                        weights.LogDictionaryValues("Leave trade deal weights");
                        Kingdom target = weights.PickRandomElementGivenWeights();

                        Debug.Log(this.name + " chose to target " + target.name);
                        PerformAction(actionToPerform, target);
                    } else {
                        Debug.Log(this.name + " tried to perform " + actionToPerform.ToString() + ", but it had no targets!", this.capitalCity.hexTile);
                    }
                }
            } else {
                WeightedDictionary<Kingdom> targetKingdomWeights = GoalManager.Instance.GetKingdomWeightsForActionType(this, actionToPerform);
                ClearRejectedOffersList(); //Clear List Since weights are already computed
                if (targetKingdomWeights.GetTotalOfWeights() > 0) {
                    targetKingdomWeights.LogDictionaryValues(actionToPerform.ToString() + " weights: ");
                    Kingdom target = targetKingdomWeights.PickRandomElementGivenWeights();
                    Debug.Log(this.name + " chose to target " + target.name);
                    PerformAction(actionToPerform, target);
                } else {
                    Debug.Log(this.name + " tried to perform " + actionToPerform.ToString() + ", but it had no targets!", this.capitalCity.hexTile);
                }
            }
        } else {
            ClearRejectedOffersList(); //Clear List Since weights are already computed
            Debug.Log(this.name + " chose to do nothing.");
        }
        Debug.Log("====================");
    }
    private void PerformAction(WEIGHTED_ACTION weightedAction, object target, Kingdom cause = null) {
        switch (weightedAction) {
            //case WEIGHTED_ACTION.WAR_OF_CONQUEST:
            //    StartWarOfConquestTowards((Kingdom)target);
            //    break;
            case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
                OfferAllianceOfConquestTo((Kingdom)target, cause);
                break;
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                OfferAllianceOfProtectionTo((Kingdom)target);
                break;
            case WEIGHTED_ACTION.TRADE_DEAL:
                OfferTradeDealTo((Kingdom)target);
                break;
            case WEIGHTED_ACTION.INCITE_UNREST:
                CreateSubterfugeEvent(SUBTERFUGE_ACTIONS.REDUCE_STABILITY, (Kingdom)target);
                break;
            case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
                CreateSubterfugeEvent(SUBTERFUGE_ACTIONS.INTERNATIONAL_INCIDENT, (Kingdom)target);
                break;
            case WEIGHTED_ACTION.FLATTER:
                CreateSubterfugeEvent(SUBTERFUGE_ACTIONS.FLATTER, (Kingdom)target);
                break;
            //case WEIGHTED_ACTION.DECLARE_PEACE:
            //    ((Warfare)target).PeaceDeclaration(this);
            //    break;
            case WEIGHTED_ACTION.LEAVE_ALLIANCE:
                LeaveAlliance();
                break;
            case WEIGHTED_ACTION.LEAVE_TRADE_DEAL:
                KingdomManager.Instance.RemoveTradeDeal(this, (Kingdom)target);
                break;
            default:
                break;
        }
    }
    internal bool IsThreatened() {
        //if i am adjacent to someone whose threat is +20 or above and whose Opinion of me is negative
        for (int i = 0; i < this.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = this.adjacentKingdoms[i];
            KingdomRelationship relWithOtherKingdom = this.GetRelationshipWithKingdom(otherKingdom);
            KingdomRelationship relOfOtherWithSource = otherKingdom.GetRelationshipWithKingdom(this);
            if (relWithOtherKingdom.targetKingdomThreatLevel >= 20 && relOfOtherWithSource.totalLike < 0) {
                return true;
            }
        }
        return false;
    }
    //internal WEIGHTED_ACTION DetermineWeightedActionToPerform() {
    //    Dictionary<WEIGHTED_ACTION, int> totalWeightedActions = new Dictionary<WEIGHTED_ACTION, int>();
    //    totalWeightedActions.Add(WEIGHTED_ACTION.DO_NOTHING, 50); //Add 500 Base Weight on Do Nothing Action
    //    for (int i = 0; i < king.allTraits.Count; i++) {
    //        Trait currTrait = king.allTraits[i];
    //        Dictionary<WEIGHTED_ACTION, int> weightsFromCurrTrait = currTrait.GetTotalActionWeights();
    //        totalWeightedActions = Utilities.MergeWeightedActionDictionaries(totalWeightedActions, weightsFromCurrTrait);
    //    }
    //    return Utilities.PickRandomElementWithWeights(totalWeightedActions);
    //}
    //internal Dictionary<Kingdom, int> GetKingdomWeightsForActionType(WEIGHTED_ACTION weightedAction) {
    //    Dictionary<Kingdom, int> kingdomWeights = new Dictionary<Kingdom, int>();
    //    for (int i = 0; i < discoveredKingdoms.Count; i++) {
    //        Kingdom otherKingdom = discoveredKingdoms[i];
    //        int weightForOtherKingdom = GetDefaultWeightForAction(weightedAction, otherKingdom);
    //        //loop through all the traits of the current king
    //        for (int j = 0; j < king.allTraits.Count; j++) {
    //            Trait currTrait = king.allTraits[j];
    //            int modificationFromTrait = currTrait.GetWeightOfActionGivenTarget(weightedAction, otherKingdom, weightForOtherKingdom);
    //            weightForOtherKingdom += modificationFromTrait;
    //        }
    //        ApplyActionModificationForAll(weightedAction, otherKingdom, ref weightForOtherKingdom);
    //        kingdomWeights.Add(otherKingdom, weightForOtherKingdom);
    //    }
    //    return kingdomWeights;
    //}

    //internal Dictionary<Kingdom, Dictionary<Kingdom, int>> GetKingdomWeightsForSpecialActionType(WEIGHTED_ACTION specialWeightedAction) {
    //    Dictionary<Kingdom, Dictionary<Kingdom, int>> kingdomWeights = new Dictionary<Kingdom, Dictionary<Kingdom, int>>();
    //    for (int i = 0; i < discoveredKingdoms.Count; i++) {
    //        Kingdom otherKingdom = discoveredKingdoms[i]; //the cause of the action
    //        Dictionary<Kingdom, int> possibleAllies = new Dictionary<Kingdom, int>();
    //        for (int j = 0; j < otherKingdom.adjacentKingdoms.Count; j++) {
    //            Kingdom adjKingdomOfOtherKingdom = otherKingdom.adjacentKingdoms[j]; //the target of the action
    //            if(adjKingdomOfOtherKingdom.id != this.id) {
    //                int weightForOtherKingdom = GetDefaultWeightForAction(specialWeightedAction, otherKingdom);
    //                //loop through all the traits of the current king
    //                for (int k = 0; k < king.allTraits.Count; k++) {
    //                    Trait currTrait = king.allTraits[k];
    //                    int modificationFromTrait = currTrait.GetWeightOfActionGivenTargetAndCause(specialWeightedAction, adjKingdomOfOtherKingdom, otherKingdom, weightForOtherKingdom);
    //                    weightForOtherKingdom += modificationFromTrait;
    //                }
    //                ApplyActionModificationForAll(specialWeightedAction, otherKingdom, ref weightForOtherKingdom);
    //                possibleAllies.Add(adjKingdomOfOtherKingdom, weightForOtherKingdom);
    //            }
    //        }            
    //        kingdomWeights.Add(otherKingdom, possibleAllies);
    //    }
    //    return kingdomWeights;
    //}

    //private int GetDefaultWeightForAction(WEIGHTED_ACTION weightedAction, Kingdom targetKingdom) {
    //    switch (weightedAction) {
    //        case WEIGHTED_ACTION.WAR_OF_CONQUEST:
    //            return 0;
    //        case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
    //            return 0;
    //        case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
    //            return 0;
    //        case WEIGHTED_ACTION.TRADE_DEAL:
    //            return GetTradeDealDefaultWeight(targetKingdom);
    //        case WEIGHTED_ACTION.INCITE_UNREST:
    //            return GetInciteUnrestDefaultWeight(targetKingdom);
    //        case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
    //            return GetInternationalIncidentDefaultWeight(targetKingdom);
    //        case WEIGHTED_ACTION.FLATTER:
    //            return GetFlatterDefaultWeight(targetKingdom);
    //        case WEIGHTED_ACTION.SEND_AID:
    //            return 0;
    //        default:
    //            return 0;
    //    }
    //}
    //private void ApplyActionModificationForAll(WEIGHTED_ACTION weightedAction, Kingdom targetKingdom, ref int defaultWeight) {
    //    switch (weightedAction) {
    //        case WEIGHTED_ACTION.WAR_OF_CONQUEST:
    //            GetAllModificationForWarOfConquest(targetKingdom, ref defaultWeight);
    //            break;
    //        case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
    //            GetAllModificationForAllianceOfProtection(targetKingdom, ref defaultWeight);
    //            break;
    //        case WEIGHTED_ACTION.TRADE_DEAL:
    //            GetAllModificationForTradeDeal(targetKingdom, ref defaultWeight);
    //            break;
    //    }
    //}

  //  private int GetTradeDealDefaultWeight(Kingdom targetKingdom) {
  //      int defaultWeight = 0;
  //      KingdomRelationship relWithOtherKingdom = this.GetRelationshipWithKingdom(targetKingdom);
  //      KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(this);

		//if (relWithOtherKingdom.sharedRelationship.isAdjacent) {
  //          defaultWeight = 40;
  //          if (relWithOtherKingdom.totalLike > 0) {
  //              defaultWeight += 2 * relWithOtherKingdom.totalLike;//add 2 to Default Weight per Positive Opinion I have towards target
  //          } else if (relWithOtherKingdom.totalLike < 0) {
  //              defaultWeight += 2 * relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
  //          }

  //          //add 1 to Default Weight per Positive Opinion target has towards me
  //          //subtract 1 to Default Weight per Negative Opinion target has towards me
  //          defaultWeight += relOfOtherWithSource.totalLike;
  //          defaultWeight = Mathf.Max(0, defaultWeight); //minimum 0

  //      }
  //      return defaultWeight;
  //  }
  //  private int GetInciteUnrestDefaultWeight(Kingdom targetKingdom) {
  //      int defaultWeight = 0;
  //      KingdomRelationship relWithOtherKingdom = this.GetRelationshipWithKingdom(targetKingdom);
  //      KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(this);

  //      if (!relWithOtherKingdom.AreAllies()) {
  //          defaultWeight = 40;
  //          if (relWithOtherKingdom.totalLike < 0) {
  //              defaultWeight += relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
  //          }
  //      }
  //      return defaultWeight;
  //  }
  //  private int GetInternationalIncidentDefaultWeight(Kingdom targetKingdom) {
  //      int defaultWeight = 0;
  //      KingdomRelationship relWithOtherKingdom = this.GetRelationshipWithKingdom(targetKingdom);
  //      if(relWithOtherKingdom.totalLike < 0) {
  //          defaultWeight += Mathf.Abs(5 * relWithOtherKingdom.totalLike);
  //      }
  //      return defaultWeight;
  //  }
  //  private int GetFlatterDefaultWeight(Kingdom targetKingdom) {
  //      int defaultWeight = 40;
  //      KingdomRelationship relOtherWithSource = targetKingdom.GetRelationshipWithKingdom(this);
  //      if (relOtherWithSource.totalLike < 0) {
  //          defaultWeight += Mathf.Abs(relOtherWithSource.totalLike);
  //      }
  //      return defaultWeight;
  //  }

  //  private void GetAllModificationForWarOfConquest(Kingdom targetKingdom, ref int defaultWeight) {
  //      KingdomRelationship relWithTargetKingdom = this.GetRelationshipWithKingdom(targetKingdom);
  //      List<Kingdom> alliesAtWarWith = relWithTargetKingdom.GetAlliesTargetKingdomIsAtWarWith();
  //      //for each non-ally adjacent kingdoms that one of my allies declared war with recently
		//if (relWithTargetKingdom.sharedRelationship.isAdjacent && !relWithTargetKingdom.AreAllies() && alliesAtWarWith.Count > 0) {
  //          //compare its theoretical power vs my theoretical power
  //          int sourceKingdomPower = relWithTargetKingdom._theoreticalPower;
  //          int otherKingdomPower = targetKingdom.GetRelationshipWithKingdom(this)._theoreticalPower;
  //          if (otherKingdomPower * 1.25f < sourceKingdomPower) {
  //              //If his theoretical power is not higher than 25% over mine
  //              defaultWeight = 20;
  //              for (int j = 0; j < alliesAtWarWith.Count; j++) {
  //                  Kingdom currAlly = alliesAtWarWith[j];
  //                  KingdomRelationship relationshipWithAlly = this.GetRelationshipWithKingdom(currAlly);
  //                  if (relationshipWithAlly.totalLike > 0) {
  //                      defaultWeight += 2 * relationshipWithAlly.totalLike; //add 2 weight per positive opinion i have over my ally
  //                  } else if (relationshipWithAlly.totalLike < 0) {
  //                      defaultWeight += relationshipWithAlly.totalLike; //subtract 1 weight per negative opinion i have over my ally (totalLike is negative)
  //                  }
  //              }
  //              //add 1 weight per negative opinion i have over the target
  //              //subtract 1 weight per positive opinion i have over the target
  //              defaultWeight += (relWithTargetKingdom.totalLike * -1); //If totalLike is negative it becomes positive(+), otherwise it becomes negative(-)
  //              defaultWeight = Mathf.Max(0, defaultWeight);
  //          }
  //      }
  //  }
  //  private void GetAllModificationForAllianceOfProtection(Kingdom targetKingdom, ref int defaultWeight) {
  //      if (this.IsThreatened()) {
  //          //loop through known Kingdoms i am not at war with and whose Opinion of me is positive
  //          KingdomRelationship relWithOtherKingdom = this.GetRelationshipWithKingdom(targetKingdom);
  //          KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(this);
		//	if (!relOfOtherWithSource.sharedRelationship.isAtWar && relOfOtherWithSource.totalLike > 0) {
  //              defaultWeight += 3 * relOfOtherWithSource.totalLike;//add 3 Weight for every positive Opinion it has towards me
  //              defaultWeight += relWithOtherKingdom.totalLike;//subtract 1 Weight for every negative Opinion I have towards it
  //              if (_recentlyRejectedOffers.ContainsKey(targetKingdom)) {
  //                  defaultWeight -= 50;
  //              }else if (_recentlyBrokenAlliancesWith.Contains(targetKingdom)) {
  //                  defaultWeight -= 50;
  //              }
  //              defaultWeight = Mathf.Max(0, defaultWeight); //minimum 0
  //          }
  //      }
  //  }
  //  private void GetAllModificationForTradeDeal(Kingdom targetKingdom, ref int defaultWeight) {
  //      Dictionary<RESOURCE_TYPE, int> deficitOfTargetKingdom = targetKingdom.GetDeficitResourcesFor(this);
  //      Dictionary<RESOURCE_TYPE, int> surplusOfThisKingdom = this.GetSurplusResourcesFor(targetKingdom);
  //      foreach (KeyValuePair<RESOURCE_TYPE, int> kvp in surplusOfThisKingdom) {
  //          RESOURCE_TYPE currSurplus = kvp.Key;
  //          int surplusAmount = kvp.Value;
  //          if (deficitOfTargetKingdom.ContainsKey(currSurplus)) {
  //              //otherKingdom has a deficit for currSurplus
  //              //add Default Weight for every point of Surplus they have on our Deficit Resources 
  //              defaultWeight += surplusAmount;
  //          }
  //      }
  //  }

    private void StartWarOfConquestTowards(Kingdom targetKingdom) {
        KingdomRelationship rel = this.GetRelationshipWithKingdom(targetKingdom);
        if (rel.AreAllies()) {
            this.LeaveAlliance();
        }
        Warfare warfare = new Warfare(this, targetKingdom, false);
        this.checkedWarfareID.Add(warfare.id);
        targetKingdom.checkedWarfareID.Add(warfare.id);
    }
    private void OfferAllianceOfProtectionTo(Kingdom targetKingdom) {
        EventCreator.Instance.CreateAllianceOfProtectionOfferEvent(this, targetKingdom);
    }
    private void OfferAllianceOfConquestTo(Kingdom targetKingdom, Kingdom cause) {
        EventCreator.Instance.CreateAllianceOfConquestOfferEvent(this, targetKingdom, cause);
    }
    private void OfferTradeDealTo(Kingdom targetKingdom) {
        EventCreator.Instance.CreateTradeDealOfferEvent(this, targetKingdom);
    }
    internal void StartInternationalIncident(Kingdom targetKingdom, string status) {
		KingdomRelationship kr = GetRelationshipWithKingdom (targetKingdom);
		if(kr.sharedRelationship.isAtWar){
			Debug.Log ("CAN'T HAVE INTERNATIONAL INCIDENT BECAUSE THEY ARE AT WAR!");
			return;
		}
		if (status.Equals("caught")) {
			InternationalIncident ii = EventCreator.Instance.CreateInternationalIncidentEvent(this, targetKingdom, this, true, true);
			ii.ShowCaughtLog();
		} else if (status.Equals("random")) {
			bool isSourceKingdomAggrieved = false;
			bool isTargetKingdomAggrieved = false;
			int chance = UnityEngine.Random.Range (0, 3);
			if(chance == 0){
				isSourceKingdomAggrieved = true;
			}else if(chance == 1){
				isTargetKingdomAggrieved = true;
			}else{
				isSourceKingdomAggrieved = true;
				isTargetKingdomAggrieved = true;
			}
			InternationalIncident ii = EventCreator.Instance.CreateInternationalIncidentEvent(this, this, targetKingdom, isSourceKingdomAggrieved, isTargetKingdomAggrieved);
			ii.ShowRandomLog();
		} else {
			Dictionary<Kingdom, int> targetWeights = GetInternationalIncidentKingdomWeights(targetKingdom);
			if (Utilities.GetTotalOfWeights(targetWeights) > 0) {
				Kingdom chosenKingdom = Utilities.PickRandomElementWithWeights(targetWeights);
				if (status.Equals("success")) {
					InternationalIncident ii = EventCreator.Instance.CreateInternationalIncidentEvent(this, targetKingdom, chosenKingdom, false, true);
					ii.ShowSuccessLog();
				} else if (status.Equals("fail")) {
					ShowInternationalIncidentFailLog(targetKingdom, chosenKingdom);
				} else if (status.Equals("critical_fail")) {
					InternationalIncident ii = EventCreator.Instance.CreateInternationalIncidentEvent(this, targetKingdom, this, true, false);
					ii.ShowCriticalFailLog(chosenKingdom);
				}
			} else {
				Debug.Log(this.name + " tried to start an INTERNATIONAL_INCIDENT but, it could not find a target for " + targetKingdom.name);
			}
		}
       
    }
    private Dictionary<Kingdom, int> GetInternationalIncidentKingdomWeights(Kingdom targetKingdom) {
        //Loop through kingdoms adjacent to the target
        Dictionary<Kingdom, int> targetWeights = new Dictionary<Kingdom, int>();
        for (int i = 0; i < targetKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = targetKingdom.adjacentKingdoms[i];
            if (otherKingdom.id != this.id) { //If Kingdom is the source, negate all Weight
                int weightForKingdom = 0;
                KingdomRelationship relOtherWithTarget = otherKingdom.GetRelationshipWithKingdom(targetKingdom);
                KingdomRelationship relTargetWithOther = targetKingdom.GetRelationshipWithKingdom(otherKingdom);
                KingdomRelationship relThisWithOther = this.GetRelationshipWithKingdom(otherKingdom);

                //Add 1 Weight for every Positive Opinion they have towards target
                if (relOtherWithTarget.totalLike > 0) {
                    weightForKingdom += relOtherWithTarget.totalLike;
                }

                if (relThisWithOther.totalLike < 0) {
                    //Add 2 Weight for every Negative Opinion I have towards it
                    weightForKingdom += Mathf.Abs(2 * relThisWithOther.totalLike);
                } else if (relThisWithOther.totalLike > 0) {
                    //Subtract 2 Weight for every Positive Opinion I have towards it
                    weightForKingdom -= 2 * relThisWithOther.totalLike;
                }
                //Add 5 Weight for every positive point of Relative Strength it has over target
                if (relTargetWithOther.relativeStrength > 0) {
                    weightForKingdom += 5 * relTargetWithOther.relativeStrength;
                }

                //If not Deceitful, subtract 100 Weight if ally
                if (!this.king.HasTrait(TRAIT.DECEITFUL)) {
                    if (relThisWithOther.AreAllies()) {
                        weightForKingdom -= 100;
                    }
                }
                targetWeights.Add(otherKingdom, weightForKingdom);
            }
        }
        return targetWeights;
    }
    private void ShowInternationalIncidentFailLog(Kingdom targetKingdom, Kingdom otherKingdom) {
        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "start_fail");
        newLog.AddToFillers(this.king, this.king.name, LOG_IDENTIFIER.KING_1);
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        newLog.AddToFillers(otherKingdom, otherKingdom.name, LOG_IDENTIFIER.KINGDOM_3);
        UIManager.Instance.ShowNotification(newLog);
    }
    internal void AddRejectedOffer(Kingdom rejectedBy, WEIGHTED_ACTION actionType) {
        if (_recentlyRejectedOffers.ContainsKey(rejectedBy)) {
            if (!_recentlyRejectedOffers[rejectedBy].Contains(actionType)) {
                _recentlyRejectedOffers[rejectedBy].Add(actionType);
            }
        } else {
            _recentlyRejectedOffers.Add(rejectedBy, new List<WEIGHTED_ACTION> { actionType });
        }
    }
    private void ClearRejectedOffersList() {
        _recentlyRejectedOffers.Clear();
    }
    private void AddRecentlyBrokenAllianceWith(Kingdom otherKingdom) {
        if (!_recentlyBrokenAlliancesWith.Contains(otherKingdom)) {
            _recentlyBrokenAlliancesWith.Add(otherKingdom);
            GameDate expiryDate = GameManager.Instance.Today();
            expiryDate.AddMonths(1);
            SchedulingManager.Instance.AddEntry(expiryDate, () => RemoveRecentlyBrokenAllianceWith(otherKingdom));
        }
    }
    private void RemoveRecentlyBrokenAllianceWith(Kingdom otherKingdom) {
        _recentlyBrokenAlliancesWith.Remove(otherKingdom);
    }

    internal List<Warfare> GetAllActiveWars() {
        List<Warfare> activeWars = new List<Warfare>();
        foreach (WarfareInfo war in warfareInfo.Values) {
            activeWars.Add(war.warfare);
        }
        return activeWars;
    }
    #endregion

	internal bool HasStillGeneralInLocation(HexTile hexTile){
		for (int i = 0; i < this.militaryManager.activeGenerals.Count; i++) {
			if(this.militaryManager.activeGenerals[i].location.id == hexTile.id){
				return true;
			}
		}
		return false;
	}

	internal int GetSurplusDeficitOfResourceType(RESOURCE_TYPE resourceType){
		if(resourceType == RESOURCE_TYPE.FOOD){
			return this._foodCityCapacity - this._cities.Count;
		}else if(resourceType == RESOURCE_TYPE.MATERIAL){
			if (this.race == RACE.HUMANS){
				return this._materialCityCapacityForHumans - this._cities.Count;
			}else if (this.race == RACE.ELVES){
				return this._materialCityCapacityForElves - this._cities.Count;
			}
		}else if(resourceType == RESOURCE_TYPE.ORE){
			return this._oreCityCapacity - this._cities.Count;
		}
		return 0;
	}

    private void IncreaseWarWearinessPerMonth() {
        if (this.isDead) {
            return;
        }

        foreach (KingdomRelationship rel in relationships.Values) {
            if(rel.sharedRelationship.warfare != null) {
                rel.sharedRelationship.warfare.AdjustWeariness(this, 1); //At the start of each month, War Weariness increases by 1
            }
        }

        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseWarWearinessPerMonth());
    }
}